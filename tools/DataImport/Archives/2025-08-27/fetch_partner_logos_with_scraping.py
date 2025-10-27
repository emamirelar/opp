#!/usr/bin/env python3
"""
Script amélioré pour récupérer les logos des organisations et drapeaux des pays
avec web scraping intégré pour remplacer l'API Logo.dev défaillante.

Utilise:
- Web scraping pour les logos d'organisations (BeautifulSoup)
- FlagCDN API pour les drapeaux de pays
"""

import csv
import json
import os
import time
import requests
from bs4 import BeautifulSoup
from datetime import datetime
from typing import Dict, List, Optional, Tuple
from urllib.parse import urlparse, quote, urljoin
import re
import random
import concurrent.futures
import threading
from requests.adapters import HTTPAdapter
from urllib3.util.retry import Retry


class WebScrapingLogoAPI:
    """Client pour récupérer les logos par web scraping avec sites web officiels depuis CSV"""
    
    def __init__(self, google_api_key: str = None, google_cx: str = None):
        self.session = requests.Session()
        
        # Configure session without retries for faster failures
        adapter = HTTPAdapter(max_retries=0)
        self.session.mount('http://', adapter)
        self.session.mount('https://', adapter)
        
        # Rotation des User-Agents pour éviter la détection
        self.user_agents = [
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0',
            'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15'
        ]
        
        # Load known domains from CSV file
        self.known_domains = self.load_domains_from_csv()
    
    def load_domains_from_csv(self) -> Dict[str, str]:
        """Load organization domains from the official_websites.csv file"""
        domains = {}
        csv_file_path = "official_websites.csv"
        
        try:
            with open(csv_file_path, 'r', encoding='utf-8') as file:
                csv_reader = csv.DictReader(file)
                for row in csv_reader:
                    org_name = row.get('Organization', '').strip()
                    website = row.get('Official Website', '').strip()
                    
                    # Only add entries that have both organization name and website
                    if org_name and website:
                        domains[org_name] = website
            
            print(f"Loaded {len(domains)} organization websites from {csv_file_path}")
            return domains
            
        except FileNotFoundError:
            print(f"Warning: {csv_file_path} not found. Using empty domains dictionary.")
            return {}
        except Exception as e:
            print(f"Error reading {csv_file_path}: {e}. Using empty domains dictionary.")
            return {}
    
    def _get_random_headers(self):
        """Génère des headers aléatoires pour éviter la détection"""
        return {
            'User-Agent': random.choice(self.user_agents),
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.5',
            'Accept-Encoding': 'gzip, deflate',
            'Connection': 'keep-alive',
            'Upgrade-Insecure-Requests': '1',
        }
    
    def try_clearbit_logo(self, domain: str) -> Optional[str]:
        """Teste l'API Clearbit comme première option"""
        try:
            # Extraire juste le domaine principal
            if domain.startswith('http'):
                domain = urlparse(domain).netloc
            domain = domain.replace('www.', '')
            
            logo_url = f"https://logo.clearbit.com/{domain}"
            response = self.session.head(logo_url, timeout=3, headers=self._get_random_headers())
            
            if response.status_code == 200:
                return logo_url
        except:
            pass
        return None
    
    def extract_logo_from_website(self, url: str) -> List[str]:
        """Extrait les URLs des logos depuis un site web"""
        try:
            self.session.headers.update(self._get_random_headers())
            response = self.session.get(url, timeout=10)
            response.raise_for_status()
            
            soup = BeautifulSoup(response.content, 'html.parser')
            logo_candidates = []
            base_url = f"{urlparse(url).scheme}://{urlparse(url).netloc}"
            
            # 1. Chercher dans les balises meta Open Graph
            og_image = soup.find('meta', property='og:image')
            if og_image and og_image.get('content'):
                logo_candidates.append(og_image['content'])
            
            # 2. Chercher les images avec 'logo' dans le nom/classe/id/alt
            for img in soup.find_all('img'):
                img_src = img.get('src', '')
                img_alt = img.get('alt', '').lower()
                img_class = ' '.join(img.get('class', [])).lower()
                img_id = img.get('id', '').lower()
                
                # Mots-clés pour identifier les logos
                logo_keywords = ['logo', 'brand', 'icon', 'symbol', 'emblem']
                
                if any(keyword in img_src.lower() + ' ' + img_alt + ' ' + img_class + ' ' + img_id 
                       for keyword in logo_keywords):
                    if img_src:
                        logo_candidates.append(img_src)
            
            # 3. Chercher dans le header et navigation
            header_sections = soup.find_all(['header', 'nav'])
            header_sections.extend(soup.find_all(class_=lambda x: x and 'header' in str(x).lower()))
            header_sections.extend(soup.find_all(class_=lambda x: x and 'navbar' in str(x).lower()))
            
            for section in header_sections:
                if section:
                    for img in section.find_all('img')[:3]:  # Max 3 images par section
                        if img.get('src'):
                            logo_candidates.append(img['src'])
            
            # 4. Chercher les favicons de haute résolution
            for link in soup.find_all('link', rel=lambda x: x and 'icon' in str(x).lower()):
                href = link.get('href')
                if href:
                    # Privilégier les favicons de grande taille
                    sizes = link.get('sizes', '')
                    if any(size in sizes for size in ['192x192', '256x256', '512x512']) or 'apple-touch' in link.get('rel', []):
                        logo_candidates.append(href)
                    elif not any('32x32' in sizes or '16x16' in sizes):  # Éviter les petites icônes
                        logo_candidates.append(href)
            
            # 5. Recherche spécialisée par classe/ID communs
            logo_selectors = [
                '.logo img', '#logo img', '.brand img', '.header-logo img',
                '.site-logo img', '.navbar-brand img', '[class*="logo"] img'
            ]
            
            for selector in logo_selectors:
                try:
                    elements = soup.select(selector)
                    for element in elements[:2]:  # Max 2 par sélecteur
                        if element.get('src'):
                            logo_candidates.append(element['src'])
                except:
                    continue
            
            # Nettoyer et compléter les URLs
            clean_candidates = []
            for candidate in logo_candidates:
                if candidate and candidate not in clean_candidates:
                    # Compléter les URLs relatives
                    if candidate.startswith('//'):
                        candidate = 'https:' + candidate
                    elif candidate.startswith('/'):
                        candidate = base_url + candidate
                    elif not candidate.startswith('http') and not candidate.startswith('data:'):
                        candidate = urljoin(url, candidate)
                    
                    # Filtrer les mauvais candidats
                    if (candidate.startswith('http') and 
                        not any(bad in candidate.lower() for bad in ['placeholder', 'default', 'spacer', 'pixel']) and
                        any(ext in candidate.lower() for ext in ['.png', '.jpg', '.jpeg', '.svg', '.gif', '.webp']) and
                        len(candidate) < 500):  # URL pas trop longue
                        clean_candidates.append(candidate)
            
            # Trier par pertinence (logos probables en premier)
            def logo_score(url):
                score = 0
                url_lower = url.lower()
                if 'logo' in url_lower: score += 10
                if 'brand' in url_lower: score += 8
                if 'icon' in url_lower: score += 5
                if any(size in url_lower for size in ['192', '256', '512']): score += 3
                if '.svg' in url_lower: score += 2
                if '.png' in url_lower: score += 1
                return score
            
            clean_candidates.sort(key=logo_score, reverse=True)
            return clean_candidates[:5]  # Top 5 candidats
            
        except Exception as e:
            print(f"      Erreur scraping {url}: {e}")
            return []
    
    def validate_logo_url(self, logo_url: str) -> bool:
        """Valide qu'une URL de logo est accessible et valide"""
        try:
            response = self.session.head(logo_url, timeout=5, headers=self._get_random_headers())
            
            if response.status_code == 200:
                content_type = response.headers.get('content-type', '').lower()
                content_length = response.headers.get('content-length')
                
                # Vérifier que c'est une image
                if any(img_type in content_type for img_type in ['image/', 'svg']):
                    # Vérifier la taille (éviter les images trop petites ou trop grandes)
                    if content_length:
                        size = int(content_length)
                        return 500 <= size <= 5_000_000  # Entre 500B et 5MB
                    return True
            
            return False
        except:
            return False
    
    def search_organization_website(self, org_name: str, category: str = None) -> Optional[str]:
        """Recherche le site web d'une organisation dans le CSV officiel uniquement"""
        
        # Chercher dans le mapping CSV officiel
        known_website = self.known_domains.get(org_name)
        if known_website:
            print(f"      📋 Site web trouvé dans le CSV officiel")
            return known_website
        
        print(f"      ❌ Organisation non trouvée dans le CSV officiel")
        return None
    
    def get_logo_with_fallback(self, organization_name: str, category: str = None) -> Optional[Dict]:
        """Récupère un logo avec multiple stratégies de fallback depuis le CSV officiel"""
        
        print(f"    🔍 Recherche de logo pour: {organization_name}")
        
        # 1. Obtenir le site web depuis le CSV officiel
        website = self.search_organization_website(organization_name, category)
        if not website:
            print(f"      ⚠️ Site web non trouvé dans le CSV officiel")
            return None
        
        print(f"      🌐 Site web: {website}")
        
        # 2. Essayer Clearbit d'abord (plus rapide)
        clearbit_logo = self.try_clearbit_logo(website)
        if clearbit_logo:
            print(f"      ✅ Logo trouvé via Clearbit")
            return {
                'logo_url': clearbit_logo,
                'website_url': website,
                'source': 'clearbit',
                'confidence': 0.9
            }
        
        # 3. Web scraping du site
        print(f"      🕷️ Scraping du site web...")
        logo_candidates = self.extract_logo_from_website(website)
        
        if logo_candidates:
            print(f"      📋 {len(logo_candidates)} candidat(s) trouvé(s)")
            
            # Valider les candidats
            for i, candidate in enumerate(logo_candidates, 1):
                print(f"      🧪 Test candidat {i}: {candidate[:80]}...")
                
                if self.validate_logo_url(candidate):
                    print(f"      ✅ Logo validé via scraping")
                    return {
                        'logo_url': candidate,
                        'website_url': website,
                        'source': 'web_scraping',
                        'confidence': 0.7,
                        'all_candidates': logo_candidates
                    }
        
        print(f"      ❌ Aucun logo valide trouvé")
        return None

class CountryCodeMapper:
    """Mappeur de codes pays (identique à la version précédente)"""
    
    def __init__(self):
        self.country_codes = {
            'AFGHANISTAN': 'af', 'ALBANIA': 'al', 'ALGERIA': 'dz', 'ANGOLA': 'ao', 'ARGENTINA': 'ar',
            'ARMENIA': 'am', 'AUSTRALIA': 'au', 'AUSTRIA': 'at', 'AZERBAIJAN': 'az', 'BAHRAIN': 'bh',
            'BANGLADESH': 'bd', 'BELARUS': 'by', 'BELGIUM': 'be', 'BENIN': 'bj', 'BOLIVIA': 'bo',
            'BOSNIA': 'ba', 'BOTSWANA': 'bw', 'BRAZIL': 'br', 'BURKINA FASO': 'bf', 'BURUNDI': 'bi',
            'CAMBODIA': 'kh', 'CAMEROON': 'cm', 'CANADA': 'ca', 'CHAD': 'td', 'CHILE': 'cl',
            'CHINA': 'cn', 'COLOMBIA': 'co', 'COSTA RICA': 'cr', 'CROATIA': 'hr', 'CUBA': 'cu',
            'CYPRUS': 'cy', 'CZECH REPUBLIC': 'cz', 'DENMARK': 'dk', 'DJIBOUTI': 'dj', 'DOMINICAN REPUBLIC': 'do',
            'ECUADOR': 'ec', 'EGYPT': 'eg', 'EL SALVADOR': 'sv', 'ESTONIA': 'ee', 'ETHIOPIA': 'et',
            'FINLAND': 'fi', 'FRANCE': 'fr', 'GABON': 'ga', 'GEORGIA': 'ge', 'GERMANY': 'de',
            'GHANA': 'gh', 'GREECE': 'gr', 'GUATEMALA': 'gt', 'GUINEA': 'gn', 'GUINEA-BISSAU': 'gw',
            'HAITI': 'ht', 'HONDURAS': 'hn', 'HUNGARY': 'hu', 'ICELAND': 'is', 'INDIA': 'in',
            'INDONESIA': 'id', 'IRAN': 'ir', 'IRAQ': 'iq', 'IRELAND': 'ie', 'ISRAEL': 'il',
            'ITALY': 'it', 'JAMAICA': 'jm', 'JAPAN': 'jp', 'JORDAN': 'jo', 'KAZAKHSTAN': 'kz',
            'KENYA': 'ke', 'KUWAIT': 'kw', 'KYRGYZSTAN': 'kg', 'LAOS': 'la', 'LATVIA': 'lv',
            'LEBANON': 'lb', 'LESOTHO': 'ls', 'LIBERIA': 'lr', 'LIBYA': 'ly', 'LITHUANIA': 'lt',
            'LUXEMBOURG': 'lu', 'MADAGASCAR': 'mg', 'MALAWI': 'mw', 'MALAYSIA': 'my', 'MALI': 'ml',
            'MALTA': 'mt', 'MAURITANIA': 'mr', 'MAURITIUS': 'mu', 'MEXICO': 'mx', 'MOLDOVA': 'md',
            'MONGOLIA': 'mn', 'MONTENEGRO': 'me', 'MOROCCO': 'ma', 'MOZAMBIQUE': 'mz', 'MYANMAR': 'mm',
            'NAMIBIA': 'na', 'NEPAL': 'np', 'NETHERLANDS': 'nl', 'NEW ZEALAND': 'nz', 'NICARAGUA': 'ni',
            'NIGER': 'ne', 'NIGERIA': 'ng', 'NORTH MACEDONIA': 'mk', 'NORWAY': 'no', 'PAKISTAN': 'pk',
            'PANAMA': 'pa', 'PARAGUAY': 'py', 'PERU': 'pe', 'PHILIPPINES': 'ph', 'POLAND': 'pl',
            'PORTUGAL': 'pt', 'QATAR': 'qa', 'ROMANIA': 'ro', 'RUSSIA': 'ru', 'RWANDA': 'rw',
            'SAUDI ARABIA': 'sa', 'SENEGAL': 'sn', 'SERBIA': 'rs', 'SIERRA LEONE': 'sl', 'SINGAPORE': 'sg',
            'SLOVAKIA': 'sk', 'SLOVENIA': 'si', 'SOMALIA': 'so', 'SOUTH AFRICA': 'za', 'SOUTH SUDAN': 'ss',
            'SPAIN': 'es', 'SRI LANKA': 'lk', 'SUDAN': 'sd', 'SWEDEN': 'se', 'SWITZERLAND': 'ch',
            'SYRIA': 'sy', 'TAJIKISTAN': 'tj', 'TANZANIA': 'tz', 'THAILAND': 'th', 'TOGO': 'tg',
            'TUNISIA': 'tn', 'TURKEY': 'tr', 'UGANDA': 'ug', 'UKRAINE': 'ua', 'UAE001': 'ae',
            'UK': 'gb', 'USA': 'us', 'URUGUAY': 'uy', 'UZBEKISTAN': 'uz', 'VENEZUELA': 've',
            'VIETNAM': 'vn', 'YEMEN': 'ye', 'ZAMBIA': 'zm', 'ZIMBABWE': 'zw'
        }
    
    def get_country_code(self, country_name: str) -> Optional[str]:
        if not country_name:
            return None
        country_upper = country_name.upper().strip()
        return self.country_codes.get(country_upper)

class FlagAPI:
    """Client pour récupérer les drapeaux de pays (identique à la version précédente)"""
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'UNOPS-Partner-Flag-Fetcher/1.0'
        })
        
        # Configure session without retries for faster failures
        adapter = HTTPAdapter(max_retries=0)
        self.session.mount('http://', adapter)
        self.session.mount('https://', adapter)
    
    def get_flag_url(self, country_code: str, size: str = 'w320') -> str:
        return f"https://flagcdn.com/{size}/{country_code.lower()}.png"
    
    def validate_flag(self, country_code: str) -> bool:
        try:
            flag_url = self.get_flag_url(country_code)
            response = self.session.head(flag_url, timeout=5)
            return response.status_code == 200
        except:
            return False

class PartnerLogoFetcher:
    """Classe principale améliorée avec web scraping et découverte automatique"""
    
    def __init__(self, google_api_key: str = None, google_cx: str = None):
        self.logo_api = WebScrapingLogoAPI(google_api_key=google_api_key, google_cx=google_cx)
        self.flag_api = FlagAPI()
        self.country_mapper = CountryCodeMapper()
        self.results = []
        self.existing_results = {}  # Cache of existing results
        self.results_lock = threading.Lock()  # Thread safety for results
        self.stats = {
            'total': 0,
            'organizations': 0,
            'countries': 0,
            'logos_found': 0,
            'flags_found': 0,
            'not_found': 0,
            'skipped': 0
        }
    
    def create_directories(self):
        """Crée les répertoires de sortie"""
        directories = ['results', 'results/logos', 'results/flags', 'results/reports']
        for directory in directories:
            os.makedirs(directory, exist_ok=True)
    
    def load_existing_results(self):
        """Charge les résultats existants pour éviter le retraitement"""
        results_file = "results/partner_logos_and_flags_results.json"
        if os.path.exists(results_file):
            try:
                with open(results_file, 'r', encoding='utf-8') as file:
                    existing_data = json.load(file)
                    
                # Index by partner_id for fast lookup
                for result in existing_data:
                    partner_id = result.get('partner_id')
                    status = result.get('status')
                    
                    # Only cache successful results and results with valid URLs
                    if partner_id and status == 'success':
                        has_logo = result.get('logo_url') is not None
                        has_flag = result.get('flag_url') is not None
                        if has_logo or has_flag:
                            self.existing_results[partner_id] = result
                
                print(f"📋 Loaded {len(self.existing_results)} existing successful results")
                
            except Exception as e:
                print(f"⚠️ Could not load existing results: {e}")
        else:
            print("📋 No existing results file found - processing all partners")
    
    def should_skip_partner(self, partner_id: str) -> bool:
        """Détermine si un partenaire doit être ignoré car déjà traité avec succès"""
        return partner_id in self.existing_results
    
    def classify_partner(self, row: Dict) -> Tuple[str, Dict]:
        """Classifie un partenaire selon son type"""
        partner_level1 = row.get('Partner_Level1', '').upper()
        partner_level3 = row.get('Partner_Level3', '').strip()
        
        if partner_level1 == 'GOVERNMENT' and partner_level3:
            return 'country', {
                'country_name': row.get('Partner_Level3_Description', partner_level3),
                'country_code': partner_level3
            }
        else:
            return 'organization', {
                'name': row.get('Partner_Description', ''),
                'short_name': row.get('Partner_Description_Short', ''),
                'category': partner_level1
            }
    
    def process_single_partner(self, partner_data: Tuple[int, str, Dict]) -> Optional[Dict]:
        """Traite un seul partenaire (thread-safe)"""
        index, partner_id, row = partner_data
        
        # Check if we should skip this partner
        if self.should_skip_partner(partner_id):
            print(f"[{index}] ⏭️  Skipping {partner_id} (already processed)")
            with self.results_lock:
                self.stats['skipped'] += 1
            return self.existing_results[partner_id]
        
        print(f"[{index}] 🔄 Processing partner {partner_id}")
        
        # Classify partner
        partner_type, type_info = self.classify_partner(row)
        
        if partner_type == 'country':
            result = self.process_country(partner_id, type_info)
            with self.results_lock:
                self.stats['countries'] += 1
        else:
            result = self.process_organization(partner_id, type_info)
            with self.results_lock:
                self.stats['organizations'] += 1
        
        return result
    
    def process_organization(self, partner_id: str, org_info: Dict) -> Dict:
        """Traite une organisation pour récupérer son logo avec scraping"""
        print(f"  🏢 Organisation: {org_info['name']}")
        
        result = {
            'partner_id': partner_id,
            'partner_name': org_info['name'],
            'partner_short_name': org_info['short_name'],
            'partner_type': 'organization',
            'category': org_info['category'],
            'logo_url': None,
            'website_url': None,
            'flag_url': None,
            'country_code': None,
            'source': None,
            'confidence': None,
            'status': 'not_found',
            'timestamp': datetime.now().isoformat()
        }
        
        # Récupérer le logo avec scraping et découverte automatique
        logo_data = self.logo_api.get_logo_with_fallback(org_info['name'], org_info['category'])
        if logo_data:
            result.update({
                'logo_url': logo_data['logo_url'],
                'website_url': logo_data['website_url'],
                'source': logo_data['source'],
                'confidence': logo_data['confidence'],
                'status': 'success'
            })
            print(f"    ✅ Logo trouvé via {logo_data['source']}")
            with self.results_lock:
                self.stats['logos_found'] += 1
        else:
            print(f"    ❌ Logo non trouvé")
            with self.results_lock:
                self.stats['not_found'] += 1
        
        return result
    
    def process_country(self, partner_id: str, country_info: Dict) -> Dict:
        """Traite un pays pour récupérer son drapeau (identique à la version précédente)"""
        print(f"  🏴 Pays: {country_info['country_name']}")
        
        result = {
            'partner_id': partner_id,
            'partner_name': country_info['country_name'],
            'partner_short_name': country_info['country_name'],
            'partner_type': 'country',
            'category': 'GOVERNMENT',
            'logo_url': None,
            'website_url': None,
            'flag_url': None,
            'country_code': None,
            'source': None,
            'confidence': None,
            'status': 'not_found',
            'timestamp': datetime.now().isoformat()
        }
        
        country_code = self.country_mapper.get_country_code(country_info['country_code'])
        if not country_code:
            country_code = self.country_mapper.get_country_code(country_info['country_name'])
        
        if country_code and self.flag_api.validate_flag(country_code):
            flag_url = self.flag_api.get_flag_url(country_code)
            result.update({
                'flag_url': flag_url,
                'country_code': country_code.upper(),
                'source': 'flagcdn',
                'confidence': 1.0,
                'status': 'success'
            })
            print(f"    ✅ Drapeau trouvé: {country_code.upper()}")
            with self.results_lock:
                self.stats['flags_found'] += 1
        else:
            print(f"    ❌ Drapeau non trouvé")
            with self.results_lock:
                self.stats['not_found'] += 1
        
        return result
    
    def process_csv_file(self, csv_path: str, max_workers: int = 4):
        """Traite le fichier CSV des partenaires avec parallélisation"""
        print(f"🚀 Lecture du fichier: {csv_path}")
        
        try:
            # Load existing results first
            self.load_existing_results()
            
            with open(csv_path, 'r', encoding='utf-8') as file:
                reader = csv.DictReader(file)
                rows = list(reader)
                
            self.stats['total'] = len(rows)
            print(f"📊 {self.stats['total']} partenaires à traiter")
            print(f"⚡ Parallélisation avec {max_workers} threads\n")
            
            # Prepare partner data for parallel processing
            partner_data_list = []
            for i, row in enumerate(rows, 1):
                partner_id = row.get('Partner', '')
                partner_data_list.append((i, partner_id, row))
            
            # Process in batches for better memory management and progress reporting
            batch_size = 20
            batches = [partner_data_list[i:i + batch_size] 
                      for i in range(0, len(partner_data_list), batch_size)]
            
            start_time = time.time()
            processed_count = 0
            
            for batch_num, batch in enumerate(batches, 1):
                print(f"📦 Processing batch {batch_num}/{len(batches)} ({len(batch)} partners)")
                
                # Process batch in parallel
                with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
                    future_to_partner = {executor.submit(self.process_single_partner, partner_data): partner_data 
                                       for partner_data in batch}
                    
                    batch_results = []
                    for future in concurrent.futures.as_completed(future_to_partner):
                        try:
                            result = future.result()
                            if result:
                                batch_results.append(result)
                                processed_count += 1
                        except Exception as e:
                            partner_data = future_to_partner[future]
                            print(f"❌ Error processing partner {partner_data[1]}: {e}")
                
                # Add batch results to main results (thread-safe)
                with self.results_lock:
                    self.results.extend(batch_results)
                
                # Progress reporting
                elapsed_time = time.time() - start_time
                rate = processed_count / elapsed_time if elapsed_time > 0 else 0
                eta = (len(partner_data_list) - processed_count) / rate if rate > 0 else 0
                
                print(f"⏱️  Processed: {processed_count}/{len(partner_data_list)} "
                      f"({processed_count/len(partner_data_list)*100:.1f}%) "
                      f"- Rate: {rate:.1f}/s - ETA: {eta/60:.1f}min")
                
                # Save intermediate results after each batch
                self.save_results(intermediate=True)
                
                # Small delay between batches to avoid overwhelming servers
                if batch_num < len(batches):
                    time.sleep(0.2)
            
            print(f"\n✅ All partners processed!")
            
        except Exception as e:
            print(f"❌ Erreur lors de la lecture du CSV: {e}")
            raise
    
    def save_results(self, intermediate: bool = False):
        """Sauvegarde les résultats"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        suffix = "_intermediate" if intermediate else ""
        
        # CSV
        csv_filename = f"results/partner_logos_and_flags_results{suffix}.csv"
        with open(csv_filename, 'w', newline='', encoding='utf-8') as file:
            if self.results:
                fieldnames = self.results[0].keys()
                writer = csv.DictWriter(file, fieldnames=fieldnames)
                writer.writeheader()
                writer.writerows(self.results)
        
        # JSON
        json_filename = f"results/partner_logos_and_flags_results{suffix}.json"
        with open(json_filename, 'w', encoding='utf-8') as file:
            json.dump(self.results, file, indent=2, ensure_ascii=False)
        
        # Rapport simplifié sans découverte automatique
        report = {
            'timestamp': timestamp,
            'statistics': self.stats,
            'summary': {
                'success_rate_logos': round((self.stats['logos_found'] / max(self.stats['organizations'], 1)) * 100, 1),
                'success_rate_flags': round((self.stats['flags_found'] / max(self.stats['countries'], 1)) * 100, 1),
                'overall_success_rate': round(((self.stats['logos_found'] + self.stats['flags_found']) / max(self.stats['total'], 1)) * 100, 1)
            }
        }
        
        report_filename = f"results/reports/processing_report{suffix}_{timestamp}.json"
        with open(report_filename, 'w', encoding='utf-8') as file:
            json.dump(report, file, indent=2, ensure_ascii=False)
        
        if not intermediate:
            print(f"\n✨ Sauvegarde terminée!")
            print(f"📁 Fichiers générés:")
            print(f"  - {csv_filename}")
            print(f"  - {json_filename}")
            print(f"  - {report_filename}")
    
    def print_final_report(self):
        """Affiche le rapport final simplifié"""
        
        print(f"\n🎯 RAPPORT FINAL")
        print(f"=" * 50)
        print(f"Total des partenaires dans le CSV: {self.stats['total']}")
        print(f"Partenaires skippés (déjà traités): {self.stats['skipped']}")
        print(f"Nouveaux partenaires traités: {self.stats['total'] - self.stats['skipped']}")
        print(f"Organisations: {self.stats['organizations']}")
        print(f"Pays: {self.stats['countries']}")
        print(f"\n📈 RÉSULTATS:")
        print(f"Logos trouvés: {self.stats['logos_found']}/{self.stats['organizations']} ({round((self.stats['logos_found'] / max(self.stats['organizations'], 1)) * 100, 1)}%)")
        print(f"Drapeaux trouvés: {self.stats['flags_found']}/{self.stats['countries']} ({round((self.stats['flags_found'] / max(self.stats['countries'], 1)) * 100, 1)}%)")
        print(f"Non trouvés: {self.stats['not_found']}")
        print(f"\n📋 SOURCE DES DONNÉES:")
        print(f"Sites web officiels: {len(self.logo_api.known_domains)} organisations dans official_websites.csv")
        print(f"Découverte automatique: Désactivée (utilise uniquement le CSV officiel)")
        print(f"\n⚡ OPTIMISATIONS:")
        print(f"Gain de temps (skip): {self.stats['skipped']} partenaires évités")
        print(f"Parallélisation: Oui (ThreadPoolExecutor)")
        print(f"Retries HTTP: Désactivés pour plus de rapidité")
        print(f"Découverte automatique: Désactivée pour plus de rapidité")
        print(f"\n🎉 Taux de succès global: {round(((self.stats['logos_found'] + self.stats['flags_found']) / max(self.stats['total'], 1)) * 100, 1)}%")

def main():
    """Fonction principale"""
    CSV_FILE = "PartnerTreeExport - TEST Combined Hierarchy 28 Aug 2025.csv"
    
    # Configuration des API de recherche (optionnelles)
    # Pour utiliser Google Custom Search, décommentez et ajoutez vos clés:
    # GOOGLE_API_KEY = "your-google-api-key"
    # GOOGLE_CX = "your-google-custom-search-engine-id"
    GOOGLE_API_KEY = None
    GOOGLE_CX = None
    
    print("🚀 UNOPS Partner Logo & Flag Fetcher (Optimisé avec CSV Officiel)")
    print("=" * 80)
    print("🔍 Fonctionnalités activées:")
    print("  - Clearbit API pour les logos")
    print("  - Web scraping des sites officiels")
    print("  - 📋 Sites web officiels depuis official_websites.csv (863+ organisations)")
    print("  - 📋 Chargement automatique des résultats existants (skip des doublons)")
    print("  - ⚡ Parallélisation des requêtes (ThreadPoolExecutor)")
    print("  - 🚫 Retries HTTP désactivés (échecs rapides)")
    print("  - ⏱️  Timeouts optimisés (3-10s au lieu de 5-15s)")
    print("  - 🚫 Découverte automatique désactivée (CSV officiel uniquement)")
    print()
    
    # Initialiser le fetcher avec les clés API
    fetcher = PartnerLogoFetcher(google_api_key=GOOGLE_API_KEY, google_cx=GOOGLE_CX)
    
    # Créer les répertoires
    fetcher.create_directories()
    
    try:
        # Traiter le fichier CSV
        fetcher.process_csv_file(CSV_FILE)
        
        # Sauvegarder les résultats finaux
        fetcher.save_results()
        
        # Afficher le rapport
        fetcher.print_final_report()
        
    except KeyboardInterrupt:
        print("\n⏹️  Arrêt demandé par l'utilisateur")
        fetcher.save_results(intermediate=True)
        fetcher.print_final_report()
    except Exception as e:
        print(f"\n❌ Erreur: {e}")
        fetcher.save_results(intermediate=True)
        raise

if __name__ == "__main__":
    main()