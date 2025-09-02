#!/usr/bin/env python3
"""
Script pour récupérer les logos des organisations et drapeaux des pays
depuis le fichier CSV des partenaires UNOPS.

Utilise:
- Logo.dev API pour les logos d'organisations
- FlagCDN API pour les drapeaux de pays
"""

import csv
import json
import os
import time
import requests
from datetime import datetime
from typing import Dict, List, Optional, Tuple
from urllib.parse import urlparse
import re

# Import the website discovery service
from website_discovery_api import WebsiteDiscoveryService

class LogoDevAPI:
    """Client pour l'API Logo.dev avec découverte automatique de sites web"""
    
    def __init__(self, api_key: str, google_api_key: str = None, google_cx: str = None):
        self.api_key = api_key
        self.base_url = "https://api.logo.dev"
        self.session = requests.Session()
        self.session.headers.update({
            'Authorization': f'Bearer {api_key}',
            'Content-Type': 'application/json',
            'User-Agent': 'UNOPS-Partner-Logo-Fetcher/1.0'
        })
        
        # Initialize website discovery service
        self.website_discovery = WebsiteDiscoveryService(
            cache_file="results/discovered_websites_cache.json"
        )
        
        # Set Google API keys if provided
        if google_api_key:
            self.website_discovery.google_api_key = google_api_key
        if google_cx:
            self.website_discovery.google_cx = google_cx
        
        # Mapping des domaines connus
        self.known_domains = {
            "DIIS Danish Institute for International Studies": "diis.dk",
            "CLAEH Latin American Centre for Human Economy": "claeh.edu.uy",
            "University of Oxford": "ox.ac.uk",
            "GELI Global Executive Leadership": "geli.org",
            "CoNISMa Consorzio Nazionale Interuniversitario per le Scienze del Mare": "conisma.it",
            "ITRC International Tuberculosis Research Center": "itrc.org",
            "IFPRI International Food Policy Research Institute": "ifpri.org",
            "ACFE Association of Certified Fraud Examiners": "acfe.com",
            "ICARDA International Center for Agricultural Research in the Dry Areas": "icarda.org",
            "SwedBio Swedish International Biodiversity Programme": "swed.bio",
            "Columbia University": "columbia.edu",
            "University of Genova": "unige.it",
            "Universidad Autónoma del Estado de Baja California": "uabc.mx",
            "LSTM Liverpool School of Tropical Medicine": "lstmed.ac.uk",
            "UPNFM National Pedagogical University Francisco Morazan": "upnfm.edu.hn",
            "University of Notre Dame": "nd.edu",
            "Loughborough University": "lboro.ac.uk",
            "UC Davis": "ucdavis.edu",
            "Stellenbosch University": "sun.ac.za",
            "The University of Sydney": "sydney.edu.au",
            "MacArthur Foundation": "macfound.org",
            "Mohammed bin Rashid Al Maktoum Foundation": "mbrf.ae",
            "Jordan River Foundation": "jordanriver.jo",
            "Nippon Foundation": "nippon-foundation.or.jp",
            "Eli Lilly and Company Foundation": "lilly.com",
            "Temasek Foundation": "temasekfoundation.org.sg",
            "MAVA Foundation": "mava-foundation.org",
            "EGPAF Elizabeth Glaser Pediatric AIDS Foundation": "pedaids.org",
            "Chinese Red Cross Foundation": "crcf.org.cn",
            "Doen Foundation": "doen.nl",
            "United Nations Foundation": "unfoundation.org",
            "FIND Foundation for Innovative New Diagnostics": "finddx.org",
            "Foundation to Promote Open Society": "opensocietyfoundations.org",
            "FHF Fred Hollows Foundation": "hollows.org",
            "Purpose Foundation": "purpose-economy.org",
            "Rockefeller Foundation": "rockefellerfoundation.org",
            "PeaceNexus Foundation": "peacenexus.org",
            "Ford Foundation": "fordfoundation.org",
            "Bill and Melinda Gates Foundation": "gatesfoundation.org",
            "GAIN Global Alliance for Improved Nutrition": "gainhealth.org",
            "IKEA Foundation": "ikeafoundation.org",
            "USAID United States Agency for International Development": "usaid.gov",
            "DFID Department For International Development": "gov.uk",
            "SIDA Swedish International Development Cooperation Agency": "sida.se",
            "SDC Swiss Agency for Development and Cooperation": "eda.admin.ch",
            "FCDO Foreign, Commonwealth & Development Office": "gov.uk"
        }
    
    def search_logo(self, query: str, limit: int = 5) -> Optional[Dict]:
        """Recherche un logo par nom d'organisation"""
        try:
            params = {
                'q': query,
                'limit': limit,
                'format': 'json'
            }
            
            response = self.session.get(
                f"{self.base_url}/v1/search",
                params=params,
                timeout=30
            )
            
            if response.status_code == 200:
                data = response.json()
                if data and 'logos' in data and data['logos']:
                    # Prendre le premier résultat (meilleure correspondance)
                    logo = data['logos'][0]
                    return {
                        'logo_url': logo.get('logo_url'),
                        'domain': logo.get('domain'),
                        'confidence': logo.get('confidence', 0),
                        'formats': logo.get('formats', []),
                        'source': 'logo_dev_search'
                    }
            
            return None
            
        except Exception as e:
            print(f"Erreur lors de la recherche Logo.dev: {e}")
            return None
    
    def get_logo_by_domain(self, domain: str) -> Optional[Dict]:
        """Récupère un logo par domaine"""
        try:
            response = self.session.get(
                f"{self.base_url}/v1/logo/{domain}",
                timeout=30
            )
            
            if response.status_code == 200:
                data = response.json()
                return {
                    'logo_url': data.get('logo_url'),
                    'domain': domain,
                    'confidence': 1.0,  # Recherche par domaine = haute confiance
                    'formats': data.get('formats', []),
                    'source': 'logo_dev_domain'
                }
            
            return None
            
        except Exception as e:
            print(f"Erreur lors de la recherche par domaine: {e}")
            return None
    
    def get_logo_with_fallback(self, organization_name: str, category: str = None) -> Optional[Dict]:
        """Essaye plusieurs méthodes pour obtenir un logo avec découverte automatique"""
        
        # 1. Essayer par domaine connu
        if organization_name in self.known_domains:
            domain = self.known_domains[organization_name]
            result = self.get_logo_by_domain(domain)
            if result:
                return result
        
        # 2. Découvrir automatiquement le site web
        print(f"      🔍 Site web non trouvé dans le mapping, utilisation de la découverte automatique...")
        website_info = self.website_discovery.discover_website(organization_name)
        
        if website_info and website_info.get('website'):
            # Extraire le domaine du site web découvert
            from urllib.parse import urlparse
            domain = urlparse(website_info['website']).netloc.replace('www.', '')
            
            print(f"      ✅ Site web découvert: {website_info['website']} (confiance: {website_info['confidence']})")
            
            # Essayer Logo.dev avec le domaine découvert
            result = self.get_logo_by_domain(domain)
            if result:
                result['discovered_website'] = website_info['website']
                result['website_confidence'] = website_info['confidence']
                result['discovery_source'] = website_info['source']
                return result
        
        # 3. Fallback: Essayer par nom complet avec l'API de recherche
        result = self.search_logo(organization_name)
        if result:
            return result
        
        # 4. Essayer avec le nom court (premier mot + dernier mot)
        words = organization_name.split()
        if len(words) > 2:
            simplified_name = f"{words[0]} {words[-1]}"
            result = self.search_logo(simplified_name)
            if result:
                return result
        
        # 5. Essayer avec juste le premier mot
        if len(words) > 1:
            result = self.search_logo(words[0])
            if result:
                return result
        
        return None

class CountryCodeMapper:
    """Mappeur de codes pays"""
    
    def __init__(self):
        # Mapping des codes pays du CSV vers codes ISO 2 lettres
        self.country_codes = {
            'AFGHANISTAN': 'af',
            'ALBANIA': 'al',
            'ALGERIA': 'dz',
            'ANGOLA': 'ao',
            'ARGENTINA': 'ar',
            'ARMENIA': 'am',
            'AUSTRALIA': 'au',
            'AUSTRIA': 'at',
            'AZERBAIJAN': 'az',
            'BAHRAIN': 'bh',
            'BANGLADESH': 'bd',
            'BELARUS': 'by',
            'BELGIUM': 'be',
            'BENIN': 'bj',
            'BOLIVIA': 'bo',
            'BOSNIA': 'ba',
            'BOTSWANA': 'bw',
            'BRAZIL': 'br',
            'BURKINA FASO': 'bf',
            'BURUNDI': 'bi',
            'CAMBODIA': 'kh',
            'CAMEROON': 'cm',
            'CANADA': 'ca',
            'CHAD': 'td',
            'CHILE': 'cl',
            'CHINA': 'cn',
            'COLOMBIA': 'co',
            'COSTA RICA': 'cr',
            'CROATIA': 'hr',
            'CUBA': 'cu',
            'CYPRUS': 'cy',
            'CZECH REPUBLIC': 'cz',
            'DENMARK': 'dk',
            'DJIBOUTI': 'dj',
            'DOMINICAN REPUBLIC': 'do',
            'ECUADOR': 'ec',
            'EGYPT': 'eg',
            'EL SALVADOR': 'sv',
            'ESTONIA': 'ee',
            'ETHIOPIA': 'et',
            'FINLAND': 'fi',
            'FRANCE': 'fr',
            'GABON': 'ga',
            'GEORGIA': 'ge',
            'GERMANY': 'de',
            'GHANA': 'gh',
            'GREECE': 'gr',
            'GUATEMALA': 'gt',
            'GUINEA': 'gn',
            'GUINEA-BISSAU': 'gw',
            'HAITI': 'ht',
            'HONDURAS': 'hn',
            'HUNGARY': 'hu',
            'ICELAND': 'is',
            'INDIA': 'in',
            'INDONESIA': 'id',
            'IRAN': 'ir',
            'IRAQ': 'iq',
            'IRELAND': 'ie',
            'ISRAEL': 'il',
            'ITALY': 'it',
            'JAMAICA': 'jm',
            'JAPAN': 'jp',
            'JORDAN': 'jo',
            'KAZAKHSTAN': 'kz',
            'KENYA': 'ke',
            'KUWAIT': 'kw',
            'KYRGYZSTAN': 'kg',
            'LAOS': 'la',
            'LATVIA': 'lv',
            'LEBANON': 'lb',
            'LESOTHO': 'ls',
            'LIBERIA': 'lr',
            'LIBYA': 'ly',
            'LITHUANIA': 'lt',
            'LUXEMBOURG': 'lu',
            'MADAGASCAR': 'mg',
            'MALAWI': 'mw',
            'MALAYSIA': 'my',
            'MALI': 'ml',
            'MALTA': 'mt',
            'MAURITANIA': 'mr',
            'MAURITIUS': 'mu',
            'MEXICO': 'mx',
            'MOLDOVA': 'md',
            'MONGOLIA': 'mn',
            'MONTENEGRO': 'me',
            'MOROCCO': 'ma',
            'MOZAMBIQUE': 'mz',
            'MYANMAR': 'mm',
            'NAMIBIA': 'na',
            'NEPAL': 'np',
            'NETHERLANDS': 'nl',
            'NEW ZEALAND': 'nz',
            'NICARAGUA': 'ni',
            'NIGER': 'ne',
            'NIGERIA': 'ng',
            'NORTH MACEDONIA': 'mk',
            'NORWAY': 'no',
            'PAKISTAN': 'pk',
            'PANAMA': 'pa',
            'PARAGUAY': 'py',
            'PERU': 'pe',
            'PHILIPPINES': 'ph',
            'POLAND': 'pl',
            'PORTUGAL': 'pt',
            'QATAR': 'qa',
            'ROMANIA': 'ro',
            'RUSSIA': 'ru',
            'RWANDA': 'rw',
            'SAUDI ARABIA': 'sa',
            'SENEGAL': 'sn',
            'SERBIA': 'rs',
            'SIERRA LEONE': 'sl',
            'SINGAPORE': 'sg',
            'SLOVAKIA': 'sk',
            'SLOVENIA': 'si',
            'SOMALIA': 'so',
            'SOUTH AFRICA': 'za',
            'SOUTH SUDAN': 'ss',
            'SPAIN': 'es',
            'SRI LANKA': 'lk',
            'SUDAN': 'sd',
            'SWEDEN': 'se',
            'SWITZERLAND': 'ch',
            'SYRIA': 'sy',
            'TAJIKISTAN': 'tj',
            'TANZANIA': 'tz',
            'THAILAND': 'th',
            'TOGO': 'tg',
            'TUNISIA': 'tn',
            'TURKEY': 'tr',
            'UGANDA': 'ug',
            'UKRAINE': 'ua',
            'UAE001': 'ae',  # United Arab Emirates
            'UK': 'gb',      # United Kingdom
            'USA': 'us',     # United States
            'URUGUAY': 'uy',
            'UZBEKISTAN': 'uz',
            'VENEZUELA': 've',
            'VIETNAM': 'vn',
            'YEMEN': 'ye',
            'ZAMBIA': 'zm',
            'ZIMBABWE': 'zw'
        }
    
    def get_country_code(self, country_name: str) -> Optional[str]:
        """Obtient le code ISO 2 lettres pour un pays"""
        if not country_name:
            return None
        
        country_upper = country_name.upper().strip()
        return self.country_codes.get(country_upper)

class FlagAPI:
    """Client pour récupérer les drapeaux de pays"""
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'UNOPS-Partner-Flag-Fetcher/1.0'
        })
    
    def get_flag_url(self, country_code: str, size: str = 'w320') -> str:
        """Génère l'URL du drapeau via FlagCDN"""
        return f"https://flagcdn.com/{size}/{country_code.lower()}.png"
    
    def validate_flag(self, country_code: str) -> bool:
        """Vérifie que le drapeau existe"""
        try:
            flag_url = self.get_flag_url(country_code)
            response = self.session.head(flag_url, timeout=10)
            return response.status_code == 200
        except:
            return False

class PartnerLogoFetcher:
    """Classe principale pour récupérer logos et drapeaux des partenaires avec découverte automatique"""
    
    def __init__(self, api_key: str, google_api_key: str = None, google_cx: str = None):
        self.logo_api = LogoDevAPI(api_key, google_api_key=google_api_key, google_cx=google_cx)
        self.flag_api = FlagAPI()
        self.country_mapper = CountryCodeMapper()
        self.results = []
        self.stats = {
            'total': 0,
            'organizations': 0,
            'countries': 0,
            'logos_found': 0,
            'flags_found': 0,
            'not_found': 0,
            'websites_discovered': 0
        }
    
    def create_directories(self):
        """Crée les répertoires de sortie"""
        directories = ['results', 'results/logos', 'results/flags', 'results/reports']
        for directory in directories:
            os.makedirs(directory, exist_ok=True)
    
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
    
    def process_organization(self, partner_id: str, org_info: Dict) -> Dict:
        """Traite une organisation pour récupérer son logo"""
        print(f"  🏢 Organisation: {org_info['name']}")
        
        result = {
            'partner_id': partner_id,
            'partner_name': org_info['name'],
            'partner_short_name': org_info['short_name'],
            'partner_type': 'organization',
            'category': org_info['category'],
            'logo_url': None,
            'flag_url': None,
            'country_code': None,
            'source': None,
            'status': 'not_found',
            'timestamp': datetime.now().isoformat()
        }
        
        # Récupérer le logo avec découverte automatique
        logo_data = self.logo_api.get_logo_with_fallback(org_info['name'], org_info['category'])
        if logo_data:
            result.update({
                'logo_url': logo_data['logo_url'],
                'source': logo_data['source'],
                'status': 'success'
            })
            print(f"    ✅ Logo trouvé via {logo_data['source']}")
            self.stats['logos_found'] += 1
        else:
            print(f"    ❌ Logo non trouvé")
            self.stats['not_found'] += 1
        
        return result
    
    def process_country(self, partner_id: str, country_info: Dict) -> Dict:
        """Traite un pays pour récupérer son drapeau"""
        print(f"  🏴 Pays: {country_info['country_name']}")
        
        result = {
            'partner_id': partner_id,
            'partner_name': country_info['country_name'],
            'partner_short_name': country_info['country_name'],
            'partner_type': 'country',
            'category': 'GOVERNMENT',
            'logo_url': None,
            'flag_url': None,
            'country_code': None,
            'source': None,
            'status': 'not_found',
            'timestamp': datetime.now().isoformat()
        }
        
        # Mapper le code pays
        country_code = self.country_mapper.get_country_code(country_info['country_code'])
        if not country_code:
            country_code = self.country_mapper.get_country_code(country_info['country_name'])
        
        if country_code:
            # Valider que le drapeau existe
            if self.flag_api.validate_flag(country_code):
                flag_url = self.flag_api.get_flag_url(country_code)
                result.update({
                    'flag_url': flag_url,
                    'country_code': country_code.upper(),
                    'source': 'flagcdn',
                    'status': 'success'
                })
                print(f"    ✅ Drapeau trouvé: {country_code.upper()}")
                self.stats['flags_found'] += 1
            else:
                print(f"    ❌ Drapeau non trouvé pour le code: {country_code}")
                self.stats['not_found'] += 1
        else:
            print(f"    ❌ Code pays non trouvé pour: {country_info['country_name']}")
            self.stats['not_found'] += 1
        
        return result
    
    def process_csv_file(self, csv_path: str):
        """Traite le fichier CSV des partenaires"""
        print(f"🚀 Lecture du fichier: {csv_path}")
        
        try:
            with open(csv_path, 'r', encoding='utf-8') as file:
                reader = csv.DictReader(file)
                rows = list(reader)
                
            self.stats['total'] = len(rows)
            print(f"📊 {self.stats['total']} partenaires à traiter\n")
            
            for i, row in enumerate(rows, 1):
                partner_id = row.get('Partner', '')
                partner_name = row.get('Partner_Description', '')
                
                print(f"[{i}/{self.stats['total']}] Traitement du partenaire {partner_id}")
                
                # Classifier le partenaire
                partner_type, type_info = self.classify_partner(row)
                
                if partner_type == 'country':
                    result = self.process_country(partner_id, type_info)
                    self.stats['countries'] += 1
                else:
                    result = self.process_organization(partner_id, type_info)
                    self.stats['organizations'] += 1
                
                self.results.append(result)
                
                # Pause pour éviter d'être bloqué
                time.sleep(0.5)
                
                # Sauvegarder les résultats intermédiaires tous les 50 partenaires
                if i % 50 == 0:
                    self.save_results(intermediate=True)
                
        except Exception as e:
            print(f"❌ Erreur lors de la lecture du CSV: {e}")
            raise
    
    def save_results(self, intermediate: bool = False):
        """Sauvegarde les résultats"""
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        suffix = "_intermediate" if intermediate else ""
        
        # Sauvegarder en CSV
        csv_filename = f"results/partner_logos_and_flags_results{suffix}.csv"
        with open(csv_filename, 'w', newline='', encoding='utf-8') as file:
            if self.results:
                fieldnames = self.results[0].keys()
                writer = csv.DictWriter(file, fieldnames=fieldnames)
                writer.writeheader()
                writer.writerows(self.results)
        
        # Sauvegarder en JSON
        json_filename = f"results/partner_logos_and_flags_results{suffix}.json"
        with open(json_filename, 'w', encoding='utf-8') as file:
            json.dump(self.results, file, indent=2, ensure_ascii=False)
        
        # Sauvegarder le rapport
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
        """Affiche le rapport final"""
        print(f"\n🎯 RAPPORT FINAL")
        print(f"=" * 50)
        print(f"Total des partenaires traités: {self.stats['total']}")
        print(f"Organisations: {self.stats['organizations']}")
        print(f"Pays: {self.stats['countries']}")
        print(f"\n📈 RÉSULTATS:")
        print(f"Logos trouvés: {self.stats['logos_found']}/{self.stats['organizations']} ({round((self.stats['logos_found'] / max(self.stats['organizations'], 1)) * 100, 1)}%)")
        print(f"Drapeaux trouvés: {self.stats['flags_found']}/{self.stats['countries']} ({round((self.stats['flags_found'] / max(self.stats['countries'], 1)) * 100, 1)}%)")
        print(f"Non trouvés: {self.stats['not_found']}")
        print(f"\n🎉 Taux de succès global: {round(((self.stats['logos_found'] + self.stats['flags_found']) / max(self.stats['total'], 1)) * 100, 1)}%")

def main():
    """Fonction principale"""
    # Configuration
    API_KEY = "sk_I02-C-0aSFWFWCGH0rZnhA"
    CSV_FILE = "PartnerTreeExport - TEST Combined Hierarchy 28 Aug 2025.csv"
    
    # Configuration des API de recherche (optionnelles)
    # Pour utiliser Google Custom Search, décommentez et ajoutez vos clés:
    # GOOGLE_API_KEY = "your-google-api-key"
    # GOOGLE_CX = "your-google-custom-search-engine-id"
    GOOGLE_API_KEY = None
    GOOGLE_CX = None
    
    print("🚀 UNOPS Partner Logo & Flag Fetcher (avec Découverte Automatique)")
    print("=" * 70)
    print("🔍 Fonctionnalités activées:")
    print("  - Logo.dev API pour les logos")
    print("  - FlagCDN API pour les drapeaux")
    print("  - Découverte automatique de sites web (DuckDuckGo + patterns)")
    if GOOGLE_API_KEY:
        print("  - Google Custom Search API")
    print("  - Cache persistant pour les sites web découverts")
    print()
    
    # Initialiser le fetcher avec les clés API
    fetcher = PartnerLogoFetcher(API_KEY, google_api_key=GOOGLE_API_KEY, google_cx=GOOGLE_CX)
    
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