-- Partner data import script
-- Generated from: partner_data_export_SF_UAT_v2 - Sheet1.csv

BEGIN;

-- Create temporary table for category mapping
CREATE TEMP TABLE temp_partner_categories AS
SELECT 
    p."Name" AS category_name,
    p."Id" AS category_id
FROM "public"."PartnerCategories" p
WHERE p."Name" IN (
    'Congo',
    'Micronesia (Federated States of)',
    'OBR-NGO (International)',
    'GLOBE International Global Legislators Organisation',
    'Romanian Angel Appeal',
    'Interpeace',
    'UNOCI United Nations Operation in Côte d''Ivoire',
    'Seychelles',
    'CEB United Nations System Chief Executives Board for Coordination',
    'Curaçao',
    'Lao People''s Democratic Republic',
    'Comoros',
    'WIPO World Intellectual Property Organization',
    'United Nations Multi-Partner Trust Fund Office',
    'MINUSCA United Nations Multidimensional Integrated Stabilization Mission in the',
    'Mongolia',
    'Montenegro',
    'Ebola Response MPTF',
    'OSAA Office of the Special Adviser on Africa',
    'ITU International Telecommunication Union',
    'DRC Pooled Fund',
    'United Nations Foundation',
    'Doen Foundation',
    'Saint Lucia',
    'UN DPPA Department of Political and Peacebuilding Affairs',
    'Algeria',
    'UNVFVT United Nations Voluntary Fund for Victims of Torture',
    'Sint Maarten (Dutch part)',
    'Slovenia',
    'Croatia',
    'Greece',
    'South Sudan',
    'Argentina',
    'CRDF Global Civilian Research and Development Foundation',
    'FTB Foreign Trade Bank of Cambodia',
    'ASEAN Association of Southeast Asian Nations',
    'New Zealand territory of Tokelau',
    'United Republic of Tanzania',
    'Other Donors',
    'UNOCA United Nations Regional Office for Central Africa',
    'GEF Global Environment Facility',
    'AFESD Arab Fund for Economic and Social Development',
    'Nansen Initiative',
    'UNSOS United Nations Support Office in Somalia',
    'Antilles',
    'Paraguay',
    'OSCE Organization for Security and Co-operation in Europe',
    'Open Society Afghanistan',
    'IFRC International Federation of Red Cross and Red Crescent Societies',
    'UNICRI United Nations Interregional Crime and Justice Research Institute',
    'UNIOGBIS United Nations Integrated Peacebuilding Office in Guinea-Bissau',
    'Sierra Leone MDTF',
    'WTO World Trade Organization',
    'Dominica',
    'Fiji',
    'CPI Community Partners International',
    'Niue',
    'Yemen Famine Relief Fund',
    'Tasmim Libya Consulting and Engineering',
    'REDD+ JP Partnership Support',
    'Lesotho',
    'Syrian Arab Republic',
    'INS-NGO (International)',
    'JP Kazakhstan Innov Aprch RPSS',
    'Nicaragua',
    'Somalia Common Humanitarian Fund',
    'Estonia',
    'South Africa',
    'Egypt',
    'PONREPP-TF Post-Nargis Response and Preparedness Plan Trust Fund',
    'Eritrea',
    'UNMISS United Nations Mission in the Republic of South Sudan',
    'Estee Lauder Companies',
    'CTBTO Preparatory Commission for the Nuclear-Test-Ban Treaty Organization',
    'Stanbic Bank Ghana',
    'GCDP Global Commission on Drug Policy',
    'MFM Menschen für Menschen',
    'Nepal - UN Peace Fund',
    'UNDEF United Nations Democracy Fund',
    'GHL Global Humanitarian Lab',
    'AKF Aga Khan Foundation',
    'Albania One UN Coherence Fund',
    'Sudan',
    'Singapore',
    'Viet Nam One Plan Fund I',
    'UNSCEAR United Nations Scientific Committee on the Effects of Atomic Radiation',
    'UNMIK United Nations Interim Administration Mission in Kosovo',
    'IRW Islamic Relief Worldwide',
    'Cities Alliance',
    'Republic of Moldova',
    'Novo Nordisk AS',
    'OPEC Organization of the Petroleum Exporting Countries',
    'Digital Good',
    'Benin',
    'ITC International Trade Centre',
    'CISCO System',
    'Kyrgyzstan One Fund',
    'Democratic Republic of the Congo',
    'PBSP Philippine Business for Social Progress',
    'CFIA United Nations Central Fund for Influenza Action',
    'CILSS Permanent Inter-State Committee on Drought Control in the Sahel',
    'PSI Population Services International',
    'UNEP United Nations Environment Programme',
    'UN EOSG Executive Office of the Secretary-General',
    'Nepal',
    'Oman',
    'UNOCI United Nations Operation in Cote d''Ivoire',
    'UNA USA United Nations Association of the USA',
    'QFFD Qatar Fund for Development',
    'CERF Central Emergency Response Fund',
    'Belarus',
    'UN ESCWA Economic and Social Commission for Western Asia',
    'IAEA International Atomic Energy Agency',
    'Association for a UN Live Museum',
    'Senegal',
    'Guinea',
    'Malawi One UN Fund',
    'Assist International',
    'RSHQ Resolute Support HQ – NATO',
    'Barbados',
    'CHAG Christian Health Association of Ghana',
    'Australia',
    'ICARDA International Center for Agricultural Research in the Dry Areas',
    'EBRD European Bank for Reconstruction and Development',
    'Switzerland',
    'MFSL Médecins Sans Frontières Logistics',
    'Bolivia (Plurinational State of)',
    'UNFIP United Nations Fund for International Partnerships',
    'Liechtenstein',
    'SkyOcean Group Holdings',
    'Ecuador',
    'UNMIL United Nations Mission in Liberia',
    'UNFCCC United Nations Framework Convention on Climate Change',
    'UN General Trust Fund',
    'JP Serbia SCILD Strengthening Capacity for Inclusive Local Development',
    'JP TFYR SNC PDV Macedonia Strengthening National Capacities to Prevent Domestic',
    'Peru',
    'Brazil',
    'UN Trust Fund to End Volence Against Women',
    'IsraAID Israel Forum for International Humanitarian Aid',
    'Belgium',
    'Albania',
    'UN ECA Economic Commission for Africa',
    'JP Kenya HIV and AIDS',
    'Maldives',
    'UN DCO United Nations Development Coordination Office',
    'KARCPP King Abdullah Relief Campaign for the Pakistani People',
    'Philips',
    'Lebanon',
    'UN Technology Bank for LDC',
    'JPF Joint Peace Fund',
    'JP DRC Microfinance II',
    'UNCCD United Nations Convention to Combat Desertification',
    'IPM Instituto de Previsión Militar',
    'Sony Group Corporation',
    'UN Trust Fund for Human Security',
    'EIF Enhanced Integrated Framework',
    'Cuba',
    'Thailand',
    'University of Notre Dame',
    'WFP United Nations World Food Programme',
    'Afghanistan',
    'UNCTAD United Nations Conference on Trade and Development',
    'DAG Development Assistance Group',
    'UPU Universal Postal Union',
    'IMF International Monetary Fund',
    'One Earth',
    'WHO / PAHO World Health Organization incl. PAHO',
    'WBG World Bank Group',
    'Israel',
    'JP Armed Violence Prevention',
    'Zambia',
    'Quadrature Climate Foundation',
    'Sri Lanka',
    'MERCOSUR Southern Common Market',
    'UNAMA United Nations Assistance Mission in Afghanistan',
    'ADB Asian Development Bank',
    'Gabon',
    'Serbia',
    'OIRSA Organismo Internacional Regional De Sanidad Agropecuaria',
    'Kuwait',
    'EBY Entidad Binacional Yacyretá',
    'JPP Somalia Joint Police Programme',
    'UNODA Office for Disarmament Affairs',
    'JP Moldova Integrated Local Development Programme',
    'Trust Territory of the Pacific Islands',
    'IUCN International Union for Conservation of Nature',
    'GAIN Global Alliance for Improved Nutrition',
    'Honduras',
    'Tanzania One UN Fund',
    'Jamaica',
    'Mott MacDonald',
    'UNOWA United Nations Office for West Africa',
    'IKEA Foundation',
    'ICAO International Civil Aviation Organization',
    'CODEMGE Minas Gerais Development Company',
    'UNICEF United Nations Children''s Fund',
    'American Red Cross',
    'LSTM Liverpool School of Tropical Medicine',
    'Mexico',
    'REALL Real Equity for All',
    'UNAMI United Nations Assistance Mission for Iraq',
    'UNAKRT United Nations Assistance to the Khmer Rouge Trials',
    'Antigua and Barbuda',
    'UNSDG United Nations Sustainable Development Group (formerly UNDG)',
    'SSACONG Congregation of the Sisters of Saint Anne',
    'IGAD Intergovernmental Authority on Development',
    'Center for Health Policies and Studies PAS Center',
    'PNG UN Country Fund',
    'Cyprus',
    'Viet Nam One Plan Fund II',
    'Chad',
    'GFATM Global Fund to Fight Aids, Tuberculosis and Malaria',
    'JP LGSP-LIC Bangladesh Local Governance Support Project – Learning and Innovatio',
    'AF Adaptation Fund',
    'UNHCR Office of the United Nations High Commissioner for Refugees',
    'Jordan',
    'Iraq',
    'Liberia',
    'Russian Federation',
    'USA United States of America',
    'Stichting Radio La Benevolencija Humanitarian Tools Foundation',
    'OTB The Office of Tony Blair',
    'GPE Global Partnership for Education',
    'Hungary',
    'Red Sea Trading Corporation Ltd.',
    'Armenia',
    'Moroco',
    'WEM Worldwide Export Management',
    'JP Timor-Leste INFUSE Inclusive Finance for Under-Served Economy',
    'FHI 360',
    'Bhutan',
    'TDH Terre des Hommes Italy',
    'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)',
    'Alter Vida',
    'Nigeria',
    'UNOAU United Nations Office to the African Union',
    '3DF Three Disease Fund',
    'UN-OHRLLS Office of the High Representative for the Least Developed Countries, L',
    'Niger',
    'ICMPD International Centre for Migration Policy Development',
    'UN-REDD Programme Fund',
    'Sustainable Markets Foundation',
    'Nordic Development Fund',
    'MINURSO United Nations Mission for the Referendum in Western Sahara',
    'UNOCT United Nations Office of Counter-Terrorism',
    'ECSAHC East, Central, and Southern Africa Health Community',
    'JP Solomon Islands PGSP Provincial Governance Strengthening Programme',
    'Equatorial Guinea',
    'CAC Central American Agricultural Council',
    'Peace Process Support - The Secretariat',
    'Community-based Based Adaptation to Climate Change',
    'UNRWA United Nations Relief and Works Agency for Palestine Refugees in the Near',
    'NDC Partnership Fund',
    'SHF Sanitation and Hygiene Fund',
    'Tearfund',
    'ARC African Risk Capacity',
    'Indonesia',
    'UNIPSIL United Nations Integrated Peacebuilding Office in Sierra Leone',
    'Denmark',
    'University of Genova',
    'ADM - PRTNR CTGRY',
    'Paul G. Allen Family Foundation',
    'Trinidad and Tobago',
    'Pacific Multi Islands',
    'UN-Water Inter-agency Trust Fund',
    'JP DRC Security Sect Reform',
    'OPCW Organisation for the Prohibition of Chemical Weapons',
    'Rwanda',
    'Monaco',
    'ACFE Association of Certified Fraud Examiners',
    'UNDP United Nations Development Programme',
    'Ford Foundation',
    'Montreal Protocol',
    'UNSCN United Nations System Standing Committee on Nutrition',
    'Yemen',
    'Petunia Foundation',
    'OSISA Open Society Initiative for Southern Africa',
    'Malaria No More',
    'Vanuatu',
    'Chinese Red Cross Foundation',
    'G5 Sahel Group of Five for the Sahel',
    'Sweden',
    'Kochon Foundation',
    'IRC International Rescue Committee',
    'MINUJUSTH United Nations Mission for Justice Support in Haiti',
    'UNROD United Nations Register of Damage',
    'SEforALL Sustainable Energy for All',
    'Other Sponsors',
    'Samoa',
    'UNIFEM United Nations Development Fund for Women',
    'ISA International Solar Alliance',
    'SSRF South Sudan Recovery Fund',
    'UN DGC Department of Global Communications',
    'Malawi',
    'New Hebrides Condominium',
    'JP Mali Agro Pastoral Products',
    'Instituto Nacional de Previsión del Magisterio INPREMA',
    'Colombia',
    'National Geographic Society',
    'IADB Inter-American Development Bank',
    'CDB Caribbean Development Bank',
    'UN DGACM Department for General Assembly and Conference Management',
    'China',
    'UN ECE Economic Commission for Europe',
    'El Salvador',
    'INT-NGO International Non-Governmental Organization',
    'CoNISMa Consorzio Nazionale Interuniversitario per le Scienze del Mare',
    'State of Palestine',
    'DCPSF Darfur Community Peace and Stability Fund',
    'Miyamoto International',
    'South Sudan Common Humanitarian Fund',
    'Philippines',
    'DIIS Danish Institute for International Studies',
    'Sequoia Climate Fund',
    'Haiti',
    'Hammer Forum',
    'Iraq UNDAF Trust Fund',
    'GCA Global Centre on Adaptation',
    'Bahrain',
    'Gambia',
    'Walmart Foundation',
    'Burundi',
    'Tajikistan',
    'UNWTO World Tourism Organization',
    'MENUB United Nations Electoral Observation Mission in Burundi',
    'Clinton Foundation',
    'UNPBF United Nations Peacebuilding Fund',
    'MacArthur Foundation',
    'Microsoft Corporation',
    'Brunei Darussalam',
    'Comic Relief',
    'Cameroon',
    'UNDG Iraq Trust Fund',
    'Lebanon Recovery Fund',
    'BINUCA United Nations Integrated Peacebuilding Office in the Central African Rep',
    'African Society for Laboratory Medicine',
    'UNGM United Nations Global Marketplace',
    'Kiribati',
    'Silatech',
    'Syria Emergency Response Fund',
    'UNIDO United Nations Industrial Development Organization',
    'PeaceNexus Foundation',
    'SwedBio Swedish International Biodiversity Programme',
    'IOM International Organization for Migration',
    'Romania',
    'UN OCHA Office for the Coordination of Humanitarian Affairs',
    'UNESCO United Nations Educational, Scientific and Cultural Organization',
    'Cook Islands',
    'IATI International Aid Transparency Initiative',
    'UN DOS Department of Operational Support',
    'Rockefeller Philanthropy Advisors',
    'JP Kosovo Domestic Violence',
    'FAO Food and Agriculture Organization of the United Nations',
    'Dominican Republic',
    'ATscale, the Global Partnership for Assistive Technology',
    'UNRISD United Nations Research Institute for Social Development',
    'UN Multi-Partner Trust Fund for Somalia (Somalia UN MPTF)',
    'Marine Information Service B.V.',
    'Chile',
    'UN DPO Department of Peace Operations',
    'JP Nepal LGCDP Local Governance and Community Development Programme',
    'Solomon Islands',
    'UN Action Against Sexual Violence in Conflict',
    'SADC Southern African Development Community',
    'Türkiye',
    'Uzbekistan',
    'Cape Verde Transition Fund',
    'Takeda Pharmaceutical Company Limited',
    'Mozambique One UN Fund',
    'Canada',
    'UPNFM National Pedagogical University Francisco Morazan',
    'BCIE Central American Bank for Economic Integration',
    'Burkina Faso',
    'Czechia',
    'IsDB Islamic Development Bank',
    'Germany',
    'ICC International Criminal Court',
    'University of Oxford',
    'Windward Fund',
    'UN Civil Society Trust Fund',
    'UNIPP United Nations Indigenous Peoples’ Partnership',
    'Save the Children',
    'Kyrgyzstan',
    'UNISFA United Nations Interim Security Force in Abyei',
    'Ukraine',
    'UNIFIL United Nations Interim Force in Lebanon',
    'CORDAID Catholic Organisation for Relief and Development Aid',
    'OBR-NGO (National)',
    'Bahamas',
    'Checci and Company Consulting',
    'ClimateWorks Foundation',
    'Hemas PLC',
    'Roche Diagnostics International AG',
    'BCBRP Meritorious Fire Department of the Republic of Panama',
    'Timor-Leste',
    'Stop TB Partnership',
    'Togo',
    'Pakistan',
    'FunziLife OY',
    'DRC Stabilization and Recovery',
    'PIFS Pacific Islands Forum Secretariat',
    'Palau',
    'CAF Development Bank of Latin America',
    'UN ECCAS Economic Community of Central African States',
    'SES Socios en Salud Sucursal Peru',
    'Namibia',
    'UNOV United Nations Office at Vienna',
    'UNMIT United Nations Integrated Mission in Timor-Leste',
    'Bill and Melinda Gates Foundation',
    'ICC International Computing Centre',
    'CBHF Clinton Bush Haiti Fund',
    'Jordan River Foundation',
    'CLAEH Latin American Centre for Human Economy',
    'Uganda',
    'Uruguay One UN Coherence Fund',
    'Cambodia',
    'Madagascar',
    'TMEA TradeMark East Africa',
    'ABDIB Associação Brasileira da Infraestrutura e Indústrias de Base',
    'Other UNDP JP',
    'UNTSO United Nations Truce Supervision',
    'AGFUND Arab Gulf Fund for Development',
    'WMO World Meteorological Organization',
    'Côte d''Ivoire',
    'Abt Associates',
    'Sheikh Eid Bin Mohammed Al Thani Charity Foundation',
    'Office of the Quartet',
    'JSI Research and Training Institute, Inc.',
    'Grenada',
    'Guinea-Bissau',
    'China, Hong Kong Special Administrative Region',
    'Iceland',
    'RAP Regimen de Aportaciones Privadas',
    'PBF Peacebuilding Fund',
    'Kiribati One UN Fund',
    'Azerbaijan',
    'Crown Agents',
    'Human Rights Mainstreaming Trust Fund',
    'Libya',
    'Joint Support to Somaliland National Electoral Commission',
    'South Korea',
    'SACEP South Asia Cooperative Environment Programme',
    'Mitsubishi',
    'LIFT Livelihoods and Food Security Fund',
    'Ireland',
    'UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women',
    'Iran (Islamic Republic of)',
    'Association IPE',
    'France',
    'UMCOR United Methodist Committee on Relief',
    'UN OHCHR Office of the United Nations High Commissioner for Human Rights',
    'Botswana UN Country Fund',
    'Lithuania',
    'United Nations Sri Lanka SDG Multi-Partner Trust Fund',
    'IFA International Fertilizer Industry Association',
    'Lesotho One UN Fund',
    'FPN Fundacion Patagonia Natural',
    'AfDB African Development Bank',
    'Democratic People''s Republic of Korea',
    'Yajilarra Trust',
    'UNON United Nations Office at Nairobi',
    'JP Timor-Leste LGSP Local Governance Support Programme',
    'GAP Foundation',
    'AmeriCares Foundation',
    'Bosnia and Herzegovina',
    'JP Liberia Gender Equality',
    'JP Guatemala Maya Programme',
    'LDSC Later Day Saints Charities',
    'Mauritius',
    'Energy Transition Partnership',
    'Austria',
    'Tunisia',
    'Zimbabwe',
    'Costa Rica',
    'Other UNDP MDTF',
    'UNTFHS United Nations Trust Fund for Human Security',
    'Suriname',
    'Millennium Promise',
    'Kosovo (under UNSCR 1244/99)',
    'Tonga',
    'Comoros One UN Fund',
    'AU African Union',
    'Montenegro UN Country Fund',
    'QRCS Qatar Red Crescent Society',
    'Global Alliance for Clean Cookstoves',
    'Guatemala',
    '3MDG/Myanmar Access for Health',
    'UN Fund for Sudano-Sahelian Activities',
    'Ethiopia One UN Fund',
    'Qatar',
    'Norway',
    'Holy See',
    'G77 Group of 77',
    'THPS Tanzania Health Promotion Support',
    'UNV United Nations Volunteers',
    'JP Guatemala Rural Dev',
    'ECOWAS Economic Community of West African States',
    'JP Chad DIS Security',
    'SUN Scaling Up Nutrition Movement',
    'Woord en Daad',
    'IFPRI International Food Policy Research Institute',
    'Mali',
    'UNODC United Nations Office on Drugs and Crime',
    'JP Liberia Food Security',
    'Panama',
    'ECEAP Estonian Center for Eastern Partnership',
    'IFAD International Fund for Agricultural Development',
    'Pakistan One Fund',
    'Central African Republic Common Humanitarian Fund',
    'Czechoslovakia',
    'UNIDIR United Nations Institute for Disarmament Research',
    'ILO International Labour Organization',
    'Tuvalu',
    'JP Somalia Local Governance and Decentralized Service Delivery',
    'MAVA Foundation',
    'Eswatini',
    'Turkmenistan',
    'UNDRR United Nations Office for Disaster Risk Reduction',
    'UNU United Nations University',
    'MINUSTAH United Nations Stabilization Mission in Haiti',
    'JP Lao Governance and Public Administration Reform',
    'Poland',
    'Malta',
    'SRSG CAAC Office of the Special Representative of the Secretary-General for Chil',
    'CAMEG Centrale d''achat des médicaments essentiels génériques et des consommables',
    'WAPCAS Ghana-West Africa Program to Combat AIDS and STI',
    'IDOR Instituto D''or De Pesquisa E Ensino',
    'Italy',
    'Bulgaria',
    'United Arab Emirates',
    'UNMOGIP United Nations Military Observer Group in India and Pakistan',
    'EU European Union',
    'Slovakia',
    'OIOS Office of Internal Oversight Services',
    'NBI Nile Basin Initiative',
    'CIFF Children''s Investment Fund Foundation',
    'Labomersa',
    'UNAIDS Joint United Nations Programme on HIV/AIDS',
    'North Macedonia',
    'ITRC International Tuberculosis Research Center',
    'Wellspring Foundation',
    'WSSCC Water Supply and Sanitation Collaborative Council',
    'NIC-Union Europea',
    'San Marino',
    'MONUSCO United Nations Organization Stabilization Mission in the Democratic Repu',
    'Amref Health Africa in Kenya',
    'GFDRR Global Facility for Disaster Reduction and Recovery',
    'Andorra',
    'Djibouti',
    'UNORCID United Nations Office for REDD+ Coordination in Indonesia',
    'KAS Konrad-Adenauer-Stiftung',
    'IATI-TF International Aid Transparency Initiative Trust Fund',
    'CRS Catholic Relief Services',
    'WALIC West Africa Livestock Innovation Centre',
    'Saint Kitts and Nevis',
    'JP Uganda Gender Equality',
    'Japan',
    'United Nations Resident Coordinator Office - Sri Lanka',
    'MINUSMA United Nations Multidimensional Integrated Stabilization Mission in Mali',
    'MDG Achievement Fund',
    'KNCV Koninklijke Nederlandse Centrale Vereniging tot bestrijding der Tuberculose',
    'Rockefeller Foundation',
    'UN DMSPC Department of Management Strategy, Policy and Compliance',
    'Bangladesh',
    'OECS Organisation of Eastern Caribbean States',
    'UNDF United Nations Fund for Recovery Reconstruction and Development in Darfur',
    'Saudi Arabia',
    'Finland',
    'DC QA',
    'Columbia University',
    'Omidyar Network',
    'UNSSC United Nations System Staff College',
    'Coca Cola Company',
    'Sierra Leone',
    'Uruguay',
    'OXFAM International',
    'GELI Global Executive Leadership',
    'UNVFTC United Nations Voluntary Fund for Technical Co-operation in the Field of',
    'OFID OPEC Fund for International Development',
    'Central African Republic',
    'New Zealand',
    'The Climate Vulnerable Forum & Vulnerable Twenty Group of Ministers of Finance',
    'Cape Verde',
    'UNFPA United Nations Population Fund',
    'FIND Foundation for Innovative New Diagnostics',
    'Sao Tome and Principe',
    'Ethiopia',
    'Netherlands',
    'India',
    'Kenya',
    'Guyana',
    'Myanmar',
    'UN ESCAP Economic and Social Commission for Asia and the Pacific',
    'UNMIS United Nations Mission in Sudan',
    'RBM Roll Back Malaria',
    'CFC Common Fund for Commodities',
    'R20 Regions of Climate Action',
    'IMO International Maritime Organization',
    'Belize',
    'Devnet International',
    'Caritas Internationalis',
    'GAVI Global Alliance for Vaccination and Immunization',
    'Macfadden',
    'Maldives One UN Fund',
    'UN Haiti Cholera Response Multi-Partner Trust Fund',
    'OECD Organisation for Economic Co-operation and Development',
    'UNFICYP United Nations Peacekeeping Force in Cyprus',
    'UN OLA Office of Legal Affairs',
    'VTF UN Voluntary Trust Fund for Assistance in Mine Action',
    'AFC Asian Football Confederation',
    'UN DESA Department of Economic and Social Affairs',
    'Papua New Guinea',
    'Indonesia Disaster Recovery Trust Fund',
    'PATH',
    'Sudan Common Humanitarian Fund',
    'Soins de santé primaires en milieu rural (SANRU)',
    'UNVFD United Nations Voluntary Fund on Disability',
    'Somalia',
    'INCAP Institute of Nutrition of Central America and Panama',
    'BEGECA Beschaffungsgesellschaft mbH',
    'UN-HABITAT United Nations Human Settlement Programme',
    'World Vision',
    'Mauritania',
    'Venezuela (Bolivarian Republic of)',
    'UNCDF United Nations Capital Development Fund',
    'UNITAR United Nations Institute for Training and Research',
    'Somalia Stability Fund',
    'Haiti Reconstruction Fund',
    'IRENA International Renewable Energy Agency',
    'UEMOA West African Economic and Monetary Union',
    'UN ECLAC Economic Commission for Latin America and the Caribbean',
    'C40 Climate Leadership Group',
    'ICAT Initiative for Climate Action Transparency',
    'Portugal',
    'Temasek Foundation',
    'UNOG United Nations Office at Geneva',
    'NAT-NGO Non-Governmental Organization',
    'Nauru',
    'Kazakhstan',
    'Saint Vincent and the Grenadines',
    'Georgia',
    'Angola',
    'Yugoslavia',
    'UK United Kingdom',
    'BCG Boston Consulting Group',
    'UNSOM United Nations Assistance Mission in Somalia',
    'CMI Center for Mediterranean Integration',
    'EGPAF Elizabeth Glaser Pediatric AIDS Foundation',
    'Itaipu Binacional',
    'UNSMIL United Nations Support Mission in Libya',
    'Ghana',
    'Eli Lilly and Company Foundation',
    'Mohammed bin Rashid Al Maktoum Foundation',
    'Mozambique',
    'Rwanda One UN Fund',
    'Spain',
    'Malaysia',
    'Botswana',
    'Marshall Islands',
    'UNITAID International Drug Purchase Facility',
    'Latvia',
    'JP Uganda Support for AIDS',
    'OIF Organisation internationale de la Francophonie',
    'Luxembourg',
    'The Defeat-NCD Partnership',
    'Nutrition International',
    'UNOPS United Nations Office for Project Services',
    'DNA Genotek',
    'Virgin Islands of the United States',
    'UNITLIFE United Nations Initiative Fighting Chronic Malnutrition Through Innovat',
    'Bhutan UN Country Fund',
    'Purpose Foundation',
    'UNOIP United Nations Office of the Iraq Programme',
    'Nippon Foundation',
    'CRPD Convention on the Rights of Persons with Disabilities',
    'UNDSS Department of Safety and Security',
    'UN United Nations',
    'Viet Nam'
);

-- Insert partner records

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1813', 'CODEMGE Minas Gerais Development Company', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CODEMGE', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CODEMGE Minas Gerais Development Company')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1540', 'Other Donors', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Other Donors', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Other Donors')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1541', 'Other Sponsors', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Other Sponsors', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Other Sponsors')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1586', 'NIC-Union Europea', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NIC-Union Europea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'NIC-Union Europea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1819', 'BCBRP Meritorious Fire Department of the Republic of Panama', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BCBRP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'BCBRP Meritorious Fire Department of the Republic of Panama')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1815', 'INCAP Institute of Nutrition of Central America and Panama', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INCAP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'INCAP Institute of Nutrition of Central America and Panama')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1786', 'IPM Instituto de Previsión Militar', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IPM', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IPM Instituto de Previsión Militar')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1784', 'Instituto Nacional de Previsión del Magisterio INPREMA', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INPREMA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Instituto Nacional de Previsión del Magisterio INPREMA')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1735', 'Nippon Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nippon Foundation', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nippon Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1759', 'AKF Aga Khan Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AKF', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AKF Aga Khan Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1750', 'IDOR Instituto D''or De Pesquisa E Ensino', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IDOR', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IDOR Instituto D''or De Pesquisa E Ensino')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1456', 'Mohammed bin Rashid Al Maktoum Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MBRF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mohammed bin Rashid Al Maktoum Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1664', 'Omidyar Network', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Omidyar Network', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Omidyar Network')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1598', 'Doen Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Doen Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Doen Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1767', 'Wellspring Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Wellspring', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Wellspring Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1722', 'REALL Real Equity for All', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'REALL', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'REALL Real Equity for All')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1690', 'Jordan River Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Jordan River Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Jordan River Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1743', 'Rockefeller Philanthropy Advisors', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'RPA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Rockefeller Philanthropy Advisors')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1146', 'PeaceNexus Foundation', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PeaceNexus Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PeaceNexus Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1755', 'Yajilarra Trust', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Yajilarra Trust', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Yajilarra Trust')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1455', 'Bill and Melinda Gates Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Gates Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bill and Melinda Gates Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1671', 'CRDF Global Civilian Research and Development Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CRDF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CRDF Global Civilian Research and Development Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1020', 'Clinton Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Clinton Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Clinton Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1829', 'Stichting Radio La Benevolencija Humanitarian Tools Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'La Benevolencija', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Stichting Radio La Benevolencija Humanitarian Tools Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1696', 'IKEA Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ikea Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IKEA Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1801', 'Sequoia Climate Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sequoia Climate Fund', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sequoia Climate Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1816', 'KAS Konrad-Adenauer-Stiftung', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KAS', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'KAS Konrad-Adenauer-Stiftung')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1684', 'EGPAF Elizabeth Glaser Pediatric AIDS Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EGPAF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EGPAF Elizabeth Glaser Pediatric AIDS Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1074', 'Paul G. Allen Family Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Paul G. Allen Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Paul G. Allen Family Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1580', 'Ford Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ford Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ford Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1147', 'Petunia Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Petunia Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Petunia Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1772', 'Purpose Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Purpose Foundation', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Purpose Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1034', 'Eli Lilly and Company Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Eli Lilly Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Eli Lilly and Company Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1594', 'GAP Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GAP Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GAP Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1795', 'Temasek Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Temasek Foundation', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Temasek Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1057', 'Kochon Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kochon Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kochon Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1071', 'Open Society Afghanistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Open Society Afghanistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Open Society Afghanistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1588', 'United Nations Foundation', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'United Nations Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Nations Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1579', 'MacArthur Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mac Arthur Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MacArthur Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1751', 'WAPCAS Ghana-West Africa Program to Combat AIDS and STI', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WAPCAS', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WAPCAS Ghana-West Africa Program to Combat AIDS and STI')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1036', 'FIND Foundation for Innovative New Diagnostics', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FIND', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FIND Foundation for Innovative New Diagnostics')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1595', 'United Nations Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Nations Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1727', 'Chinese Red Cross Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CRCF', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Chinese Red Cross Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1446', 'GAIN Global Alliance for Improved Nutrition', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GAIN', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GAIN Global Alliance for Improved Nutrition')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1749', 'ECEAP Estonian Center for Eastern Partnership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ECEAP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ECEAP Estonian Center for Eastern Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1016', 'CIFF Children''s Investment Fund Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CIFF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CIFF Children''s Investment Fund Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1768', 'Quadrature Climate Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QCF', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Quadrature Climate Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1770', 'Yemen Famine Relief Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Famine Relief Fund', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Yemen Famine Relief Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1645', 'Walmart Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Walmart Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Walmart Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1585', 'Rockefeller Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Rockefeller Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Rockefeller Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1748', 'MAVA Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MAVA Foundation', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MAVA Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1149', 'R20 Regions of Climate Action', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'R20', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'R20 Regions of Climate Action')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1711', 'Fleming Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Fleming Fund', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1144', 'United Kingdom of Great Britain and Northern Ireland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UK', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1618', 'British overseas territory of Montserrat', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Montserrat', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1613', 'British overseas territory of Cayman Islands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cayman Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1111', 'DFID Department For International Development', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DFID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1752', 'FCDO Foreign, Commonwealth & Development Office', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FCDO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1610', 'British overseas territory of Anguilla', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Anguilla', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1145', 'United States of America', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1115', 'USDA United States Department of Agriculture', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1788', 'U.S. Department of State’s Bureau of International Narcotics and Law Enforcement Affairs INL', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USDOS-INL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1112', 'USAID United States Agency for International Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USAID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1642', 'USDOC United States Department of Commerce', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USDOC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1641', 'USAID and Affiliated U.S. Agency for International Development and Affiliated', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USAID & Affiliated', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1113', 'USDOS United States Department of State', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'USDOS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1116', 'OFDA Office of U.S. Foreign Disaster Assistance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OFDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1702', 'CDC United States Centers for Disease Control and Prevention', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CDC US', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1114', 'MCC Millennium Challenge Corporation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MCC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'USA United States of America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1266', 'Italy', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Italy', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Italy')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1268', 'Albania', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Albania', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Albania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1269', 'Algeria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Algeria', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Algeria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1270', 'Andorra', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Andorra', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Andorra')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1271', 'Angola', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Angola', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Angola')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1272', 'Antigua and Barbuda', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Antigua and Barbuda', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Antigua and Barbuda')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1273', 'Argentina', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Argentina', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Argentina')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1274', 'Armenia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Armenia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Armenia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1275', 'Azerbaijan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Azerbaijan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Azerbaijan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1276', 'Bahamas', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bahamas', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bahamas')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1277', 'Bahrain', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bahrain', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bahrain')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1278', 'Bangladesh', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bangladesh', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bangladesh')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1279', 'Barbados', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Barbados', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Barbados')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1280', 'Belarus', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Belarus', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Belarus')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1281', 'Belize', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Belize', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Belize')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1282', 'Benin', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Benin', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Benin')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1283', 'Bhutan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bhutan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bhutan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1284', 'Bolivia (Plurinational State of)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bolivia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bolivia (Plurinational State of)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1285', 'Bosnia and Herzegovina', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bosnia and Herzegovina', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bosnia and Herzegovina')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1286', 'Botswana', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Botswana', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Botswana')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1287', 'Brunei Darussalam', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Brunei Darussalam', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Brunei Darussalam')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1288', 'Bulgaria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bulgaria', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bulgaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1289', 'Burkina Faso', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Burkina Faso', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Burkina Faso')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1290', 'Burundi', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Burundi', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Burundi')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1291', 'Democratic People''s Republic of Korea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DPR Korea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Democratic People''s Republic of Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1292', 'Cameroon', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cameroon', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cameroon')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1293', 'Cape Verde', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cape Verde', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cape Verde')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1294', 'Central African Republic', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Central African Republic', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Central African Republic')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1295', 'Chad', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Chad', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Chad')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1296', 'Chile', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Chile', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Chile')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1297', 'Colombia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Colombia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Colombia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1298', 'Comoros', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Comoros', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Comoros')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1299', 'Cook Islands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cook Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cook Islands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1300', 'Democratic Republic of the Congo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DR Congo', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Democratic Republic of the Congo')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1302', 'Croatia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Croatia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Croatia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1303', 'Cuba', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cuba', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cuba')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1304', 'Cyprus', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cyprus', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cyprus')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1305', 'Czechia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Czech Republic', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Czechia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1306', 'Ghana', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ghana', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ghana')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1307', 'Jordan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Jordan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Jordan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1308', 'Kazakhstan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kazakhstan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kazakhstan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1309', 'Grenada', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Grenada', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Grenada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1310', 'Kenya', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kenya', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kenya')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1311', 'Kiribati', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kiribati', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kiribati')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1312', 'Kuwait', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kuwait', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kuwait')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1313', 'Kyrgyzstan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kyrgyzstan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kyrgyzstan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1314', 'Lao People''s Democratic Republic', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lao PDR', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lao People''s Democratic Republic')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1315', 'Latvia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Latvia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Latvia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1316', 'Lebanon', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lebanon', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lebanon')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1317', 'Lesotho', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lesotho', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lesotho')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1318', 'Liberia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Liberia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Liberia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1319', 'Oman', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Oman', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Oman')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1320', 'Pakistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Pakistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Pakistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1321', 'Palau', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Palau', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Palau')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1323', 'Papua New Guinea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Papua New Guinea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Papua New Guinea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1326', 'Philippines', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Philippines', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Philippines')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1327', 'Afghanistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Afghanistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Afghanistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1328', 'Cambodia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cambodia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cambodia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1329', 'Côte d''Ivoire', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Côte d''Ivoire', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Côte d''Ivoire')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1330', 'Congo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Congo', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Congo')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1331', 'Djibouti', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Djibouti', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Djibouti')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1332', 'Dominica', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Dominica', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Dominica')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1333', 'Dominican Republic', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Dominican Republic', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Dominican Republic')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1334', 'Ecuador', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ecuador', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ecuador')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1335', 'Egypt', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Egypt', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Egypt')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1336', 'El Salvador', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'El Salvador', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'El Salvador')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1337', 'Equatorial Guinea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Equatorial Guinea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Equatorial Guinea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1338', 'Eritrea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Eritrea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Eritrea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1340', 'Estonia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Estonia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Estonia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1342', 'Fiji', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Fiji', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Fiji')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1343', 'Haiti', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Haiti', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Haiti')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1345', 'Iraq', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Iraq', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iraq')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1346', 'Gabon', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Gabon', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Gabon')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1347', 'Gambia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Gambia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Gambia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1348', 'Georgia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Georgia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Georgia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1349', 'Guatemala', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Guatemala', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Guatemala')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1350', 'Guinea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Guinea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Guinea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1351', 'Guyana', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Guyana', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Guyana')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1352', 'Indonesia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Indonesia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Indonesia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1353', 'Jamaica', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Jamaica', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Jamaica')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1354', 'Lithuania', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lithuania', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lithuania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1355', 'Iran (Islamic Republic of)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Iran', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iran (Islamic Republic of)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1356', 'Holy See', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Holy See', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Holy See')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1358', 'Hungary', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hungary', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Hungary')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1359', 'Kosovo (under UNSCR 1244/99)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kosovo', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kosovo (under UNSCR 1244/99)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1360', 'Madagascar', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Madagascar', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Madagascar')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1361', 'Malawi', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Malawi', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Malawi')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1362', 'Malaysia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Malaysia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Malaysia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1363', 'Maldives', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Maldives', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Maldives')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1364', 'Mali', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mali', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mali')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1365', 'Malta', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Malta', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Malta')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1366', 'Marshall Islands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Marshall Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Marshall Islands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1367', 'Mauritania', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mauritania', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mauritania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1368', 'Mauritius', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mauritius', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mauritius')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1369', 'Mexico', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mexico', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mexico')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1370', 'Micronesia (Federated States of)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Micronesia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Micronesia (Federated States of)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1372', 'South Sudan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'South Sudan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Sudan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1373', 'Monaco', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Monaco', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Monaco')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1374', 'Montenegro', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Montenegro', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Montenegro')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1376', 'Mozambique', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mozambique', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mozambique')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1377', 'Namibia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Namibia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Namibia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1378', 'Nauru', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nauru', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nauru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1379', 'Niger', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Niger', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Niger')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1380', 'Nepal', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nepal', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nepal')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1381', 'Mongolia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mongolia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mongolia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1382', 'Myanmar', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Myanmar', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Myanmar')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1383', 'Nicaragua', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nicaragua', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nicaragua')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1384', 'Nigeria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nigeria', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nigeria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1385', 'Republic of Moldova', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Moldova', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Republic of Moldova')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1386', 'Romania', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Romania', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Romania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1387', 'Russian Federation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Russia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Russian Federation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1388', 'Rwanda', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Rwanda', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Rwanda')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1389', 'Saint Kitts and Nevis', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Saint Kitts and Nevis', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saint Kitts and Nevis')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1390', 'Saint Lucia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Saint Lucia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saint Lucia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1391', 'Saint Vincent and the Grenadines', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Saint Vincent and the Grenadines', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saint Vincent and the Grenadines')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1392', 'Samoa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Samoa', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Samoa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1393', 'San Marino', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'San Marino', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'San Marino')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1394', 'Sao Tome and Principe', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sao Tome and Principe', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sao Tome and Principe')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1396', 'Senegal', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Senegal', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Senegal')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1397', 'Suriname', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Suriname', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Suriname')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1399', 'Serbia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Serbia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Serbia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1400', 'Seychelles', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Seychelles', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Seychelles')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1401', 'Sierra Leone', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sierra Leone', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sierra Leone')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1402', 'Singapore', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Singapore', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Singapore')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1403', 'Slovakia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Slovakia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Slovakia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1404', 'Slovenia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Slovenia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Slovenia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1405', 'Solomon Islands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Solomon Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Solomon Islands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1406', 'Somalia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Somalia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Somalia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1407', 'Sri Lanka', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sri Lanka', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sri Lanka')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1408', 'Sudan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sudan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sudan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1409', 'State of Palestine', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'State of Palestine', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'State of Palestine')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1410', 'Syrian Arab Republic', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Syria', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Syrian Arab Republic')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1411', 'Tajikistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tajikistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tajikistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1412', 'Thailand', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Thailand', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Thailand')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1413', 'North Macedonia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'North Macedonia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'North Macedonia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1414', 'Timor-Leste', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Timor-Leste', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Timor-Leste')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1415', 'Togo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Togo', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Togo')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1416', 'Tonga', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tonga', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tonga')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1417', 'Trinidad and Tobago', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Trinidad and Tobago', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Trinidad and Tobago')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1418', 'Tunisia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tunisia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tunisia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1419', 'United Republic of Tanzania', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tanzania', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Republic of Tanzania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1420', 'Guinea-Bissau', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Guinea-Bissau', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Guinea-Bissau')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1421', 'Turkmenistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Turkmenistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Turkmenistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1422', 'Tuvalu', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tuvalu', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tuvalu')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1423', 'Uganda', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Uganda', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Uganda')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1425', 'United Arab Emirates', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'United Arab Emirates', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Arab Emirates')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1426', 'Uruguay', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Uruguay', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Uruguay')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1427', 'Uzbekistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Uzbekistan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Uzbekistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1428', 'Vanuatu', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Vanuatu', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Vanuatu')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1429', 'Venezuela (Bolivarian Republic of)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Venezuela', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Venezuela (Bolivarian Republic of)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1430', 'Viet Nam', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Viet Nam', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Viet Nam')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1431', 'Yemen', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Yemen', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Yemen')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1432', 'Zambia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Zambia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Zambia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1433', 'Zimbabwe', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Zimbabwe', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Zimbabwe')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1575', 'Yugoslavia', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Yugoslavia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Yugoslavia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1612', 'Virgin Islands of the United States', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Virgin Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Virgin Islands of the United States')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1614', 'Czechoslovakia', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Czechoslovakia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Czechoslovakia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1615', 'Antilles', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Antilles', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Antilles')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1617', 'China, Hong Kong Special Administrative Region', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hong Kong', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'China, Hong Kong Special Administrative Region')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1619', 'New Hebrides Condominium', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'New Hebrides Condominium', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'New Hebrides Condominium')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1620', 'Niue', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Niue', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Niue')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1621', 'Pacific Multi Islands', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Pacific Multi Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Pacific Multi Islands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1624', 'New Zealand territory of Tokelau', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'New Zealand territory of Tokelau', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'New Zealand territory of Tokelau')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1625', 'Trust Territory of the Pacific Islands', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Trust Territory of the Pacific Islands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Trust Territory of the Pacific Islands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1677', 'Uganda', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Uganda', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Uganda')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1715', 'Curaçao', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Curaçao', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Curaçao')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1716', 'Sint Maarten (Dutch part)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sint Maarten', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sint Maarten (Dutch part)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1818', 'QFFD Qatar Fund for Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QFFD', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'QFFD Qatar Fund for Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1780', 'AMSAC Activos Mineros S.A.C.', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AMSAC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Peru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1325', 'Peru', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Peru', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Peru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1783', 'PNSU Programa Nacional de Saneamiento Urbano', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PNSU', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Peru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1724', 'Sedapal', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sedapal', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Peru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1728', 'China National Pharmaceutical Group Southwest Medicine Co., Ltd.', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SINOPHARM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'China')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1085', 'MOFCOM Ministry of Commerce of the People''s Republic of China', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MOFCOM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'China')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1122', 'China', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'China', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'China')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1623', 'Indian State of Sikkim', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Indian State of Sikkim', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'India')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1344', 'India', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'India', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'India')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1131', 'Japan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Japan', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Japan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1095', 'JICA Japan International Cooperation Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JICA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Japan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1096', 'JBIC Japan Bank for International Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JBIC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Japan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1139', 'Republic of Korea', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Republic of Korea', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1105', 'KOICA Korea International Cooperation Agency', 'Active', 'Allowed', '231123', NULL, NULL, NULL, NULL, NULL, NULL, 'KOICA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1339', 'Libya', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Libya', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Libya')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1707', 'Libyan Presidency Council', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Libyan Presidency Council', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Libya')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1371', 'Qatar', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Qatar', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Qatar')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1141', 'Spain', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Spain', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Spain')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1107', 'AECID Spanish Agency for International Development Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AECID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Spain')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1119', 'Brazil', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Brazil', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Brazil')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1079', 'ABC Agência Brasileira de Cooperação', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ABC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Brazil')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1084', 'IDRC International Development Research Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IDRC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Canada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1082', 'CIDA Canadian International Development Agency', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CIDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Canada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1024', 'CAD Global Affairs Canada', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Global Affairs Canada', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Canada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1083', 'DFAIT Department of Foreign Affairs and International Trade Canada', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DFAIT - Canada', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Canada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1121', 'Canada', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Canada', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Canada')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1125', 'France', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'France', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'France')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1088', 'AFD French Development Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AFD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'France')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1622', 'French overseas department of Réunion', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Réunion', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'France')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1616', 'French overseas department of Guadeloupe', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Guadeloupe', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'France')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1823', 'NAWEC National Water and Electricity Company Ltd - Gambia', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NAWEC Gambia', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Gambia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1090', 'Hellenic Aid', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hellenic Aid', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Greece')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1127', 'Greece', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Greece', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Greece')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1093', 'MASHAV Center for International Cooperation of the Foreign Ministry of Israel', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MASHAV', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Israel')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1130', 'Israel', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Israel', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Israel')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1753', 'Ministry of Justice of Norway', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoJ Norway', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Norway')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1136', 'Norway', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Norway', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Norway')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1102', 'Ministry of Foreign Affairs of Norway', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoFA Norway', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Norway')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1101', 'NORAD Norwegian Agency for Development Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NORAD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Norway')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1322', 'Panama', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Panama', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Panama')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1781', 'INADEH Instituto Nacional de Formacion Profesional y Capacitacion para el Desarrollo Humano', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INADEH', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Panama')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1137', 'Poland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Poland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Poland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1103', 'Polish Aid', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Polish Aid', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Poland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1754', 'SEPA Swedish Environmental Protection Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SEPA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sweden')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1108', 'SIDA Swedish International Development Cooperation Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SIDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sweden')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1267', 'Sweden', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sweden', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sweden')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1143', 'Türkiye', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Türkiye', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Türkiye')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1110', 'TIKA Turkish Cooperation and Coordination Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TIKA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Türkiye')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1077', 'ADA Austria Development Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ADA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Austria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1118', 'Austria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Austria', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Austria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1078', 'AWS Austria Wirtschaftsservice Gesellschaft', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AWS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Austria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1120', 'Belgium', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Belgium', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Belgium')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1080', 'ENABEL Belgian Development Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ENABEL', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Belgium')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1081', 'FPS Ministry of Foreign Affairs, Foreign Trade and Development Cooperation of Belgium', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FPS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Belgium')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1123', 'Denmark', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Denmark', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Denmark')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1086', 'DANIDA Danish International Development Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DANIDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Denmark')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1087', 'Ministry for Foreign Affairs of Finland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoFA Finland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Finland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1124', 'Finland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Finland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Finland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1737', 'AA German Federal Foreign Office (Auswärtiges Amt)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoFA Germany', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1126', 'Germany', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Germany', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1738', 'BMUV German Federal Ministry for the Environment, Nature Conservation, Nuclear Safety and Consumer Protection', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BMUV', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1089', 'BMZ German Federal Ministry for Economic Cooperation and Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BMZ', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1589', 'GIZ German Corporation for International Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GIZ', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1669', 'KfW Kreditanstalt für Wiederaufbau', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KfW', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Germany')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1091', 'Ministry of Foreign Affairs of Iceland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoFA Iceland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iceland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1128', 'Iceland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Iceland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iceland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1049', 'ICEIDA Icelandic International Development Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICEIDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iceland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1092', 'IrishAid', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Irish Aid', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ireland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1129', 'Ireland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ireland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ireland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1678', 'MRPSC Ministère chargé des relations avec le Parlement et la Société Civile', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MRPSC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Moroco')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1375', 'Morocco', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Morocco', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Moroco')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1828', 'Ukrainian Railways (Ukrzaliznytsia -UZ)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ukrainian Railways', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ukraine')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1424', 'Ukraine', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ukraine', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ukraine')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1827', 'NERCHA - The National Emergency Response Council on HIV and AIDS of Eswatini', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NERCHA - Eswatini', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Eswatini')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1398', 'Eswatini', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Eswatini', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Eswatini')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1341', 'Ethiopia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ethiopia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ethiopia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1805', 'ECAE Ethiopian Conformity Assessment Enterprise', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ECAE Ethiopia', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ethiopia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1799', 'EPSS Ethiopian Pharmaceuticals Supply Service', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EPSS Ethiopia', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ethiopia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1803', 'EPHI Ethiopian Public Health Institute', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EPHI Ethiopia', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ethiopia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1357', 'Honduras', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Honduras', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Honduras')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1787', 'Instituto Hondureño de Seguridad Social IHSS', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IHSS', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Honduras')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1785', 'ENP Empresa Nacional Portuaria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ENP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Honduras')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1773', 'ANDE Administración Nacional de Electricidad', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ANDE', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Paraguay')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1324', 'Paraguay', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Paraguay', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Paraguay')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1798', 'DNA Paraguayan Dirección Nacional de Aduanas of Paraguay', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DNA Paraguay', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Paraguay')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1104', 'IPAD Portuguese Institute for Development Support', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IPAD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Portugal')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1138', 'Portugal', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Portugal', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Portugal')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1117', 'Australia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Australia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Australia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1757', 'Australian Department of Defence', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DoD Australia', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Australia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1076', 'DFAT Department of Foreign Affairs and Trade', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DFAT - Australia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Australia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1822', 'INVU Instituto Nacional de Vivienda y Urbanismo Costa Rica', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INVU', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Costa Rica')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1301', 'Costa Rica', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Costa Rica', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Costa Rica')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1812', 'INS National Insurance Institute of Costa Rica', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INS Costa Rica', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Costa Rica')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1133', 'Luxembourg', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Luxembourg', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Luxembourg')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1098', 'LuxDev Luxembourg Agency for Development Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'LuxDev', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Luxembourg')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1134', 'Netherlands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Netherlands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Netherlands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1611', 'Aruba (Kingdom of the Netherlands)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Aruba', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Netherlands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1099', 'Ministry of Foreign Affairs of the Netherlands', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MoFA Netherlands', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Netherlands')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1100', 'NZAID New Zealand Agency for International Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NZAID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'New Zealand')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1135', 'New Zealand', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'New Zealand', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'UNOPS administers', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'New Zealand')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1142', 'Switzerland', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Switzerland', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Switzerland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1109', 'SDC Swiss Agency for Development and Cooperation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SDC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Switzerland')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1395', 'Saudi Arabia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Saudi Arabia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saudi Arabia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1723', 'SFD Saudi Fund for Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SFD', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saudi Arabia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1761', 'KSRelief King Salman Humanitarian Aid and Relief Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KSRelief', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Saudi Arabia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1023', 'DBSA Development Bank of Southern Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DBSA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1106', 'SADPA South African Development Partnership Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SADPA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1140', 'South Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'South Africa', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1097', 'LED Liechtenstein Development Service', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'LED', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Liechtenstein')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1132', 'Liechtenstein', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Liechtenstein', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Liechtenstein')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1158', 'Tasmim Libya Consulting and Engineering', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tasmim Libya Consulting', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tasmim Libya Consulting and Engineering')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1691', 'FunziLife OY', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FunziLife OY', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FunziLife OY')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1742', 'Takeda Pharmaceutical Company Limited', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Takeda', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Takeda Pharmaceutical Company Limited')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1720', 'Macfadden', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Macfadden', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Macfadden')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1703', 'Roche Diagnostics International AG', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Roche Diagnostics', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Roche Diagnostics International AG')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1065', 'Mitsubishi', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mitsubishi', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mitsubishi')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1596', 'Microsoft Corporation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Microsoft', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Microsoft Corporation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1073', 'OTB The Office of Tony Blair', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OTB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OTB The Office of Tony Blair')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1810', 'Labomersa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Labomersa', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Labomersa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1022', 'Crown Agents', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Crown Agents', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Crown Agents')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1002', 'Accenture', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Accenture', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ECEAP Estonian Center for Eastern Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1734', 'Stanbic Bank Ghana', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Stanbic Bank Ghana', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Stanbic Bank Ghana')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1688', 'Novo Nordisk AS', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Novo Nordisk', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Novo Nordisk AS')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1712', 'ABDIB Associação Brasileira da Infraestrutura e Indústrias de Base', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ABDIB', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ABDIB Associação Brasileira da Infraestrutura e Indústrias de Base')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1656', 'BEGECA Beschaffungsgesellschaft mbH', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BEGECA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'BEGECA Beschaffungsgesellschaft mbH')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1741', 'RAP Regimen de Aportaciones Privadas', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'RAP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'RAP Regimen de Aportaciones Privadas')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1710', 'Red Sea Trading Corporation Ltd.', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Red Sea Trading Corporation', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Red Sea Trading Corporation Ltd.')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1067', 'Mott MacDonald', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mott MacDonald', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mott MacDonald')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1745', 'Estee Lauder Companies', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Estee Lauder Companies', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Estee Lauder Companies')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1008', 'BCG Boston Consulting Group', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Boston Consulting Group', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'BCG Boston Consulting Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1746', 'Abt Associates', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Abt Associates', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Abt Associates')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1153', 'SkyOcean Group Holdings', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SkyOcean', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SkyOcean Group Holdings')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1693', 'Miyamoto International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Miyamoto International', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Miyamoto International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1602', 'Coca Cola Company', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Coca Cola Company', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Coca Cola Company')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1670', 'DNA Genotek', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DNA Genotek', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DNA Genotek')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1777', 'Sony Group Corporation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SONY', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sony Group Corporation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1457', 'Hemas PLC', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hemas PLC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Hemas PLC')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1599', 'Checci and Company Consulting', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Checci and Company', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Checci and Company Consulting')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1605', 'Marine Information Service B.V.', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Marine Information Service', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Marine Information Service B.V.')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1600', 'CISCO System', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CISCO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CISCO System')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1038', 'FTB Foreign Trade Bank of Cambodia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Foreign Trade Bank of Cambodia', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FTB Foreign Trade Bank of Cambodia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1148', 'Philips', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Philips', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Philips')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1657', 'WEM Worldwide Export Management', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WEM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WEM Worldwide Export Management')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1015', 'South Sudan Common Humanitarian Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SSHF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Sudan Common Humanitarian Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1027', 'Ebola Response MPTF', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ebola Response MPTF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ebola Response MPTF')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1151', 'Syria Emergency Response Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Syria Emergency Response Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Syria Emergency Response Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1154', 'UN Multi-Partner Trust Fund for Somalia (Somalia UN MPTF)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Somalia UN MPTF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Multi-Partner Trust Fund for Somalia (Somalia UN MPTF)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1166', 'UNDF United Nations Fund for Recovery Reconstruction and Development in Darfur', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDF United Nations Fund for Recovery Reconstruction and Development in Darfur')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1168', 'UN General Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN General Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN General Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1226', 'CERF Central Emergency Response Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CERF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CERF Central Emergency Response Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1237', 'UNPBF United Nations Peacebuilding Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNPBF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNPBF United Nations Peacebuilding Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1239', 'UNVFTC United Nations Voluntary Fund for Technical Co-operation in the Field of Human Rights', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNVFTC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNVFTC United Nations Voluntary Fund for Technical Co-operation in the Field of')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1240', 'UNVFVT United Nations Voluntary Fund for Victims of Torture', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNVFVT', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNVFVT United Nations Voluntary Fund for Victims of Torture')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1241', 'UNVFD United Nations Voluntary Fund on Disability', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNVFD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNVFD United Nations Voluntary Fund on Disability')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1255', 'UNDEF United Nations Democracy Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDEF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDEF United Nations Democracy Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1258', 'UNFIP United Nations Fund for International Partnerships', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNFIP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNFIP United Nations Fund for International Partnerships')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1463', 'UN-Water Inter-agency Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN-Water', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN-Water Inter-agency Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1464', 'Albania One UN Coherence Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Albania One UN Coherence Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Albania One UN Coherence Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1465', 'Bhutan UN Country Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Bhutan UN Country Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Bhutan UN Country Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1466', 'Botswana UN Country Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Botswana UN Country Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Botswana UN Country Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1467', 'Cape Verde Transition Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cape Verde Transition Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cape Verde Transition Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1468', 'Central African Republic Common Humanitarian Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CAR HF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Central African Republic Common Humanitarian Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1469', 'CFIA United Nations Central Fund for Influenza Action', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CFIA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CFIA United Nations Central Fund for Influenza Action')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1470', 'Community-based Based Adaptation to Climate Change', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Community-based Based Adaptation to Clim', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Community-based Based Adaptation to Climate Change')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1471', 'Comoros One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Comoros One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Comoros One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1472', 'DCPSF Darfur Community Peace and Stability Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DCPSF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DCPSF Darfur Community Peace and Stability Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1473', 'DRC Pooled Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DRC Pooled Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DRC Pooled Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1474', 'DRC Stabilization and Recovery', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DRC Stabilization and Recovery', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DRC Stabilization and Recovery')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1475', 'Ethiopia One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Ethiopia One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Ethiopia One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1476', 'Human Rights Mainstreaming Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Human Rights Mainstreaming Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Human Rights Mainstreaming Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1477', 'Indonesia Disaster Recovery Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Indonesia Disaster Recovery Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Indonesia Disaster Recovery Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1478', 'Iraq UNDAF Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Iraq UNDAF Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Iraq UNDAF Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1479', 'JP Armed Violence Prevention', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Armed Violence Prevention', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Armed Violence Prevention')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1480', 'JP LGSP-LIC Bangladesh Local Governance Support Project – Learning and Innovation Component', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Bangladesh LGSP–LIC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP LGSP-LIC Bangladesh Local Governance Support Project – Learning and Innovatio')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1481', 'JP Chad DIS Security', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Chad DIS Security', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Chad DIS Security')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1482', 'JP DRC Microfinance II', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP DRC Microfinance II', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP DRC Microfinance II')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1483', 'JP DRC Security Sect Reform', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP DRC Security Sect Reform', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP DRC Security Sect Reform')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1484', 'JP Guatemala Maya Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Guatemala Maya Programme', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Guatemala Maya Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1485', 'JP Guatemala Rural Dev', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Guatemala Rural Dev', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Guatemala Rural Dev')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1486', 'JP Kazakhstan Innov Aprch RPSS', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Kazakhstan Innov Aprch RPSS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Kazakhstan Innov Aprch RPSS')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1487', 'JP Kenya HIV and AIDS', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Kenya HIV and AIDS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Kenya HIV and AIDS')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1488', 'JP Kosovo Domestic Violence', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Kosovo Domestic Violence', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Kosovo Domestic Violence')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1489', 'JP Lao Governance and Public Administration Reform', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Lao Govern/Public Admin', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Lao Governance and Public Administration Reform')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1490', 'JP Liberia Food Security', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Liberia Food Security', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Liberia Food Security')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1491', 'JP Liberia Gender Equality', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Liberia Gender Equality', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Liberia Gender Equality')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1492', 'JP Mali Agro Pastoral Products', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Mali Agro Pastoral Products', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Mali Agro Pastoral Products')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1493', 'JP Moldova Integrated Local Development Programme', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Moldova JILDP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Moldova Integrated Local Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1494', 'JP Nepal LGCDP Local Governance and Community Development Programme', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Nepal LGCDP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Nepal LGCDP Local Governance and Community Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1495', 'JP Serbia SCILD Strengthening Capacity for Inclusive Local Development', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Serbia SCILD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Serbia SCILD Strengthening Capacity for Inclusive Local Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1496', 'JP Solomon Islands PGSP Provincial Governance Strengthening Programme', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Solomon Islands PGSP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Solomon Islands PGSP Provincial Governance Strengthening Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1497', 'JP Somalia Local Governance and Decentralized Service Delivery', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Somalia Loc Gov and Decentral', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Somalia Local Governance and Decentralized Service Delivery')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1498', 'JP TFYR SNC PDV Macedonia Strengthening National Capacities to Prevent Domestic Violence', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP TFYR Macedonia Dom Violence', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP TFYR SNC PDV Macedonia Strengthening National Capacities to Prevent Domestic')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1499', 'JP Timor-Leste INFUSE Inclusive Finance for Under-Served Economy', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Timor-Leste INFUSE', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Timor-Leste INFUSE Inclusive Finance for Under-Served Economy')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1500', 'JP Timor-Leste LGSP Local Governance Support Programme', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Timor-Leste LGSP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Timor-Leste LGSP Local Governance Support Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1501', 'JP Uganda Gender Equality', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Uganda Gender Equality', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Uganda Gender Equality')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1502', 'JP Uganda Support for AIDS', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JP Uganda Support for AIDS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JP Uganda Support for AIDS')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1503', 'Kiribati One UN Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kiribati One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kiribati One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1504', 'Kyrgyzstan One Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Kyrgyzstan One Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Kyrgyzstan One Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1505', 'Lebanon Recovery Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lebanon Recovery Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lebanon Recovery Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1506', 'Lesotho One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Lesotho One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Lesotho One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1507', 'Malawi One UN Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Malawi One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Malawi One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1508', 'Maldives One UN Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Maldives One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Maldives One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1509', 'MDG Achievement Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MDG Achievement Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MDG Achievement Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1510', 'Montenegro UN Country Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Montenegro UN Country Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Montenegro UN Country Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1511', 'Mozambique One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Mozambique One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Mozambique One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1512', 'Nepal - UN Peace Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nepal - UN Peace Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nepal - UN Peace Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1513', 'Pakistan One Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Pakistan One Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Pakistan One Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1514', 'PBF Peacebuilding Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PBF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PBF Peacebuilding Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1515', 'PNG UN Country Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PNG UN Country Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PNG UN Country Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1516', 'REDD+ JP Partnership Support', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'REDD+ JP Partnership Support', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'REDD+ JP Partnership Support')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1517', 'Rwanda One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Rwanda One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Rwanda One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1518', 'Sierra Leone MDTF', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sierra Leone MDTF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sierra Leone MDTF')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1519', 'Somalia Common Humanitarian Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Somalia Common Humanitarian Fd', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Somalia Common Humanitarian Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1520', 'SSRF South Sudan Recovery Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SSRF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SSRF South Sudan Recovery Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1521', 'Sudan Common Humanitarian Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sudan Common Humanitarian Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sudan Common Humanitarian Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1522', 'Tanzania One UN Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tanzania One UN Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tanzania One UN Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1523', 'UN Action Against Sexual Violence in Conflict', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Action', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Action Against Sexual Violence in Conflict')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1524', 'UN Civil Society Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Civil Society Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Civil Society Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1525', 'UNIPP United Nations Indigenous Peoples’ Partnership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIPP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIPP United Nations Indigenous Peoples’ Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1526', 'UN Trust Fund for Human Security', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNTFHS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Trust Fund for Human Security')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1527', 'UN Trust Fund to End Volence Against Women', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Trust Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Trust Fund to End Volence Against Women')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1528', 'Haiti Reconstruction Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDG HRF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Haiti Reconstruction Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1529', 'UNDG Iraq Trust Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDG ITF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDG Iraq Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1530', 'UN-REDD Programme Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN-REDD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN-REDD Programme Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1531', 'Uruguay One UN Coherence Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Uruguay One UN Coherence Fund', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Uruguay One UN Coherence Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1532', 'Viet Nam One Plan Fund I', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Viet Nam One Plan Fund I', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Viet Nam One Plan Fund I')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1533', 'Viet Nam One Plan Fund II', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Viet Nam One Plan Fund II', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Viet Nam One Plan Fund II')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1538', 'Other UNDP MDTF', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Other UNDP MDTF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Other UNDP MDTF')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1539', 'Other UNDP JP', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Other UNDP JP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Other UNDP JP')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1545', 'UN Fund for Sudano-Sahelian Activities', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Fund for Sudano-Sahelian Activities')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1643', 'VTF UN Voluntary Trust Fund for Assistance in Mine Action', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN VTF', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'VTF UN Voluntary Trust Fund for Assistance in Mine Action')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1705', 'UN Haiti Cholera Response Multi-Partner Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Haiti Cholera MPTF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Haiti Cholera Response Multi-Partner Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1718', 'UNTFHS United Nations Trust Fund for Human Security', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNTFHS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNTFHS United Nations Trust Fund for Human Security')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1760', 'UNITLIFE United Nations Initiative Fighting Chronic Malnutrition Through Innovation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNITLIFE', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNITLIFE United Nations Initiative Fighting Chronic Malnutrition Through Innovat')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1765', 'United Nations Multi-Partner Trust Fund Office', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN MPTF Office', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Nations Multi-Partner Trust Fund Office')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1779', 'United Nations Sri Lanka SDG Multi-Partner Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Sri Lanka SDG MPTF', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Nations Sri Lanka SDG Multi-Partner Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1234', 'UNSDG United Nations Sustainable Development Group (formerly UNDG)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDG', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSDG United Nations Sustainable Development Group (formerly UNDG)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1014', 'CEB United Nations System Chief Executives Board for Coordination', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CEB', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CEB United Nations System Chief Executives Board for Coordination')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1764', 'United Nations Resident Coordinator Office - Sri Lanka', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNRCo - Sri Lanka', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'United Nations Resident Coordinator Office - Sri Lanka')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1245', 'IAEA International Atomic Energy Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IAEA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IAEA International Atomic Energy Agency')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1252', 'OPCW Organisation for the Prohibition of Chemical Weapons', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OPCW', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OPCW Organisation for the Prohibition of Chemical Weapons')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1576', 'IOM International Organization for Migration', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IOM', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IOM International Organization for Migration')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1265', 'WTO World Trade Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WTO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WTO World Trade Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1221', 'UNAIDS Joint United Nations Programme on HIV/AIDS', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAIDS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNAIDS Joint United Nations Programme on HIV/AIDS')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1563', 'UNDP - Regional Bureau for Latin America and Carribbean', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP RBLAC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1187', 'UNDP - MSA Bilateral Donors', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - MSA Bilateral Donors', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1554', 'UNDP - Office of Communications', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - OC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1197', 'UNV United Nations Volunteers', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNV', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNV United Nations Volunteers')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1550', 'UNDP - Human Development Report Office', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP HDRO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1556', 'UNDP - Development Group Office', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - UNDG', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1775', 'UNDP - Task Force on Nature Related Disclosures', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - TNFD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1198', 'WFP United Nations World Food Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WFP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WFP United Nations World Food Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1191', 'UNDP - MSA Trust Funds', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - MSA Trust Funds', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1185', 'UNICEF United Nations Children''s Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNICEF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNICEF United Nations Children''s Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1570', 'UNDP United Nations Development Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1551', 'UNDP - Office of Audit and Investigations', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP OAI', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1562', 'UNDP - Regional Bureau for Arab States', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP RBAS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1193', 'UN-HABITAT United Nations Human Settlement Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN-HABITAT', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN-HABITAT United Nations Human Settlement Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1190', 'UNDP - MSA Recipient Governments', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - MSA Recipient Governments', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1195', 'UNFPA United Nations Population Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNFPA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNFPA United Nations Population Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1559', 'UNDP - Bureau for Management Services', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP BMS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1189', 'UNDP - MSA NGOs', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - MSA NGOs', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1555', 'UNDP - Independent Evaluation Office', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP IEO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1566', 'UNDP Global Environmental Finance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP GEF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1549', 'UNDP - Operations Support Group', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP OSG', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1548', 'UNDP - Office of the Administrator', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - Office of the Administrator', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1558', 'UNDP - Bureau for Policy and Programme Support', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP BPPS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1762', 'UN Technology Bank for LDC', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN TBLDC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN Technology Bank for LDC')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1188', 'UNDP - MSA Lending Institutions', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - MSA Lending Institutions', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1557', 'UNDP - Crisis Bureau', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP CB', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1561', 'UNDP - Regional Bureau for Asia and the Pacific', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP RBAP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1184', 'UNCDF United Nations Capital Development Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNCDF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNCDF United Nations Capital Development Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1553', 'UNDP - Bureau for External Relations and Advocacy', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP BERA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1565', 'IAPSO Inter-Agency Procurement Services Organization', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IAPSO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1552', 'UNDP - Special Unit for South-South Cooperation', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP UNSSC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1651', 'UNDP cash-based', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP - Other', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1564', 'UNDP - Regional Bureau for Europe and CIS', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP RBEC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1192', 'UNEP United Nations Environment Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNEP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNEP United Nations Environment Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1560', 'UNDP - Regional Bureau for Africa', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDP RBA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDP United Nations Development Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9006', 'Cost recovery - reserve', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cost recovery - reserve', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1233', 'UN Web Buy', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Web Buy', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1609', 'UNOPS United Nations Office for Project Services', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOPS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '8001', 'S3I Social Impact Investment Initiatives', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'S3I', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '6) Thematic Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9008', 'Pooled admin resources', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Pooled admin resources', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9011', 'UNOPS Crowd Funding pool', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOPS Crowd Funding pool', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1220', 'ICC International Computing Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ICC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICC International Computing Centre')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1056', 'KIP-TF Knowledge, Innovation and Policies for Territorial Development Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KIP-TF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1758', 'UNGM United Nations Global Marketplace', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNGM', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNGM United Nations Global Marketplace')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9007', 'Cost recovery deferred revenue', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cost recovery deferred revenue', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1183', 'UNHCR Office of the United Nations High Commissioner for Refugees', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNHCR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNHCR Office of the United Nations High Commissioner for Refugees')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9010', 'CLP Defect Liability Project', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CLP Defect Liability Project', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1196', 'UNRWA United Nations Relief and Works Agency for Palestine Refugees in the Near East', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNRWA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNRWA United Nations Relief and Works Agency for Palestine Refugees in the Near')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1652', 'Web Buy Pay in Advance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Web Buy Pay in Advance', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1461', 'UNOPS Trust Fund Management', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOPS Trust Fund Management', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1186', 'UNCTAD United Nations Conference on Trade and Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNCTAD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNCTAD United Nations Conference on Trade and Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1222', 'UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN WOMEN', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN WOMEN United Nations Entity for Gender Equality and the Empowerment of Women')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1182', 'ITC International Trade Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ITC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ITC International Trade Centre')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1629', 'UNIFEM United Nations Development Fund for Women', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIFEM', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIFEM United Nations Development Fund for Women')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9009', 'Treasury and Investment', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Treasury and Investment', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9999', 'UNOPS Special Projects', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOPS Special Projects', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1230', 'UN ECLAC Economic Commission for Latin America and the Caribbean', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ECLAC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ECLAC Economic Commission for Latin America and the Caribbean')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1235', 'UN ECE Economic Commission for Europe', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ECE', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ECE Economic Commission for Europe')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1228', 'UN ESCWA Economic and Social Commission for Western Asia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ESCWA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ESCWA Economic and Social Commission for Western Asia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1229', 'UN ECA Economic Commission for Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ECA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ECA Economic Commission for Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1227', 'UN ESCAP Economic and Social Commission for Asia and the Pacific', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ESCAP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ESCAP Economic and Social Commission for Asia and the Pacific')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1542', 'UNOIP United Nations Office of the Iraq Programme', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOIP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOIP United Nations Office of the Iraq Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1238', 'UNSCN United Nations System Standing Committee on Nutrition', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSCN', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSCN United Nations System Standing Committee on Nutrition')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1685', 'MINUJUSTH United Nations Mission for Justice Support in Haiti', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MINUJUSTH', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MINUJUSTH United Nations Mission for Justice Support in Haiti')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1590', 'UNMIK United Nations Interim Administration Mission in Kosovo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMIK', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMIK United Nations Interim Administration Mission in Kosovo')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1171', 'UNISFA United Nations Interim Security Force in Abyei', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNISFA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNISFA United Nations Interim Security Force in Abyei')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1162', 'UNAKRT United Nations Assistance to the Khmer Rouge Trials', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAKRT', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNAKRT United Nations Assistance to the Khmer Rouge Trials')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1637', 'UNSOS United Nations Support Office in Somalia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSOS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSOS United Nations Support Office in Somalia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1062', 'MINUSCA United Nations Multidimensional Integrated Stabilization Mission in the Central African Republic', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MINUSCA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MINUSCA United Nations Multidimensional Integrated Stabilization Mission in the')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1061', 'MINURSO United Nations Mission for the Referendum in Western Sahara', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MINURSO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MINURSO United Nations Mission for the Referendum in Western Sahara')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1170', 'UNIPSIL United Nations Integrated Peacebuilding Office in Sierra Leone', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIPSIL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIPSIL United Nations Integrated Peacebuilding Office in Sierra Leone')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1543', 'UNROD United Nations Register of Damage', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNROD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNROD United Nations Register of Damage')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1633', 'UNSCEAR United Nations Scientific Committee on the Effects of Atomic Radiation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSCEAR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSCEAR United Nations Scientific Committee on the Effects of Atomic Radiation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1164', 'UNAMI United Nations Assistance Mission for Iraq', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAMI', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNAMI United Nations Assistance Mission for Iraq')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1175', 'UNMIL United Nations Mission in Liberia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMIL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMIL United Nations Mission in Liberia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1066', 'MONUSCO United Nations Organization Stabilization Mission in the Democratic Republic of the Congo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MONUSCO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MONUSCO United Nations Organization Stabilization Mission in the Democratic Repu')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1058', 'MENUB United Nations Electoral Observation Mission in Burundi', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MENUB', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MENUB United Nations Electoral Observation Mission in Burundi')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1769', 'UNOCT United Nations Office of Counter-Terrorism', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOCT', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOCT United Nations Office of Counter-Terrorism')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1639', 'UNTSO United Nations Truce Supervision', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNTSO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNTSO United Nations Truce Supervision')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1630', 'UNORCID United Nations Office for REDD+ Coordination in Indonesia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNORCID', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNORCID United Nations Office for REDD+ Coordination in Indonesia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1169', 'UNIFIL United Nations Interim Force in Lebanon', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIFIL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIFIL United Nations Interim Force in Lebanon')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1236', 'UNIOGBIS United Nations Integrated Peacebuilding Office in Guinea-Bissau', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIOGBIS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIOGBIS United Nations Integrated Peacebuilding Office in Guinea-Bissau')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1176', 'UNMISS United Nations Mission in the Republic of South Sudan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMISS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMISS United Nations Mission in the Republic of South Sudan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1163', 'UNAMA United Nations Assistance Mission in Afghanistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAMA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNAMA United Nations Assistance Mission in Afghanistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1063', 'MINUSMA United Nations Multidimensional Integrated Stabilization Mission in Mali', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MINUSMA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MINUSMA United Nations Multidimensional Integrated Stabilization Mission in Mali')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1009', 'BINUCA United Nations Integrated Peacebuilding Office in the Central African Republic', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BINUCA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'BINUCA United Nations Integrated Peacebuilding Office in the Central African Rep')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1638', 'UNSOM United Nations Assistance Mission in Somalia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSOM', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSOM United Nations Assistance Mission in Somalia')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1636', 'UNSMIL United Nations Support Mission in Libya', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSMIL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSMIL United Nations Support Mission in Libya')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1178', 'UNMOGIP United Nations Military Observer Group in India and Pakistan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMOGIP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMOGIP United Nations Military Observer Group in India and Pakistan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1181', 'UNOCI United Nations Operation in Côte d''Ivoire', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOCI', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOCI United Nations Operation in Côte d''Ivoire')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1064', 'MINUSTAH United Nations Stabilization Mission in Haiti', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MINUSTAH', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MINUSTAH United Nations Stabilization Mission in Haiti')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1593', 'UNOCI United Nations Operation in Cote d''Ivoire', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOCI', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOCI United Nations Operation in Cote d''Ivoire')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1177', 'UNMIT United Nations Integrated Mission in Timor-Leste', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMIT', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMIT United Nations Integrated Mission in Timor-Leste')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1167', 'UNFICYP United Nations Peacekeeping Force in Cyprus', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNFICYP', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNFICYP United Nations Peacekeeping Force in Cyprus')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1567', 'UNMIS United Nations Mission in Sudan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMIS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNMIS United Nations Mission in Sudan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1732', 'Pulse Lab Jakarta', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Pulse Lab Jakarta', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN EOSG Executive Office of the Secretary-General')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1608', 'UN United Nations', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'United Nations', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN United Nations')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1208', 'UN OHCHR Office of the United Nations High Commissioner for Human Rights', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN OHCHR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN OHCHR Office of the United Nations High Commissioner for Human Rights')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1218', 'UN DPO Department of Peace Operations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DPO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPO Department of Peace Operations')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1209', 'OIOS Office of Internal Oversight Services', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OIOS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OIOS Office of Internal Oversight Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1726', 'UNAOC United Nations Alliance of Civilizations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAOC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN EOSG Executive Office of the Secretary-General')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1725', 'UN DCO United Nations Development Coordination Office', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DCO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DCO United Nations Development Coordination Office')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1075', 'UN DPPA Department of Political Affairs and Peacebuilding', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DPPA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPPA Department of Political and Peacebuilding Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1203', 'UN DMSPC Department of Management Strategy, Policy and Compliance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DMSPC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DMSPC Department of Management Strategy, Policy and Compliance')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1194', 'UNODC United Nations Office on Drugs and Crime', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNODC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNODC United Nations Office on Drugs and Crime')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1211', 'OSAA Office of the Special Adviser on Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OSAA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OSAA Office of the Special Adviser on Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1216', 'UNON United Nations Office at Nairobi', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNON', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNON United Nations Office at Nairobi')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1200', 'UN DESA Department of Economic and Social Affairs', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DESA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DESA Department of Economic and Social Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1708', 'United Nations Global Compact', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Global Compact', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN EOSG Executive Office of the Secretary-General')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1733', 'United Nations Global Pulse', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Global Pulse', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN EOSG Executive Office of the Secretary-General')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1706', 'UNGSC United Nations Global Service Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNGSC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DOS Department of Operational Support')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1631', 'UNOWA United Nations Office for West Africa', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOWA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOWA United Nations Office for West Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1201', 'UN DOS Department of Operational Support', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DOS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DOS Department of Operational Support')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1210', 'UN OLA Office of Legal Affairs', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN OLA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN OLA Office of Legal Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1207', 'UN OCHA Office for the Coordination of Humanitarian Affairs', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN OCHA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN OCHA Office for the Coordination of Humanitarian Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1634', 'UNSCO Office of the United Nations Special Coordinator for the Middle East', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSCO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPPA Department of Political and Peacebuilding Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1219', 'UNMAS United Nations Mine Action Service', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNMAS', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPO Department of Peace Operations')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1215', 'UN-OHRLLS Office of the High Representative for the Least Developed Countries, Landlocked Developing Countries and Small Island Developing States', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN-OHRLLS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN-OHRLLS Office of the High Representative for the Least Developed Countries, L')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1205', 'UN DGC Department of Global Communications', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DGC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DGC Department of Global Communications')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1217', 'UNOV United Nations Office at Vienna', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOV', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOV United Nations Office at Vienna')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1199', 'UN EOSG Executive Office of the Secretary-General', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN EOSG', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN EOSG Executive Office of the Secretary-General')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1223', 'UNDRR United Nations Office for Disaster Risk Reduction', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDRR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDRR United Nations Office for Disaster Risk Reduction')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1213', 'UNODA Office for Disarmament Affairs', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNODA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNODA Office for Disarmament Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1206', 'UNDSS Department of Safety and Security', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNDSS', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNDSS Department of Safety and Security')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1202', 'UN DGACM Department for General Assembly and Conference Management', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DGACM', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DGACM Department for General Assembly and Conference Management')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1180', 'UNOCA United Nations Regional Office for Central Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOCA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOCA United Nations Regional Office for Central Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1214', 'UNOG United Nations Office at Geneva', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOG', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOG United Nations Office at Geneva')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1632', 'UNRCCA United Nations Regional Centre for Preventive Diplomacy for Central Asia', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNRCCA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPPA Department of Political and Peacebuilding Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1204', 'UN DPPA Department of Political Affairs and Peacebuilding', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN DPPA', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPPA Department of Political and Peacebuilding Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1212', 'SRSG CAAC Office of the Special Representative of the Secretary-General for Children and Armed Conflict', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SRSG CAAC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SRSG CAAC Office of the Special Representative of the Secretary-General for Chil')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1179', 'UNOAU United Nations Office to the African Union', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNOAU', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOAU United Nations Office to the African Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1635', 'UNSCOL Office of the United Nations Special Coordinator for Lebanon', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSCOL', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN DPPA Department of Political and Peacebuilding Affairs')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1537', 'UNRISD United Nations Research Institute for Social Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNRISD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNRISD United Nations Research Institute for Social Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1534', 'UNIDIR United Nations Institute for Disarmament Research', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIDIR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIDIR United Nations Institute for Disarmament Research')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1224', 'UNSSC United Nations System Staff College', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSSC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNSSC United Nations System Staff College')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1535', 'UNITAR United Nations Institute for Training and Research', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNITAR', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNITAR United Nations Institute for Training and Research')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1225', 'UNU United Nations University', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNU', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNU United Nations University')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1536', 'UNICRI United Nations Interregional Crime and Justice Research Institute', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNICRI', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNICRI United Nations Interregional Crime and Justice Research Institute')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1254', 'UNCCD United Nations Convention to Combat Desertification', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNCCD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNCCD United Nations Convention to Combat Desertification')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1257', 'UNFCCC United Nations Framework Convention on Climate Change', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNFCCC', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNFCCC United Nations Framework Convention on Climate Change')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1243', 'CRPD Convention on the Rights of Persons with Disabilities', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CRPD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CRPD Convention on the Rights of Persons with Disabilities')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1259', 'UNIDO United Nations Industrial Development Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNIDO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNIDO United Nations Industrial Development Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1256', 'UNESCO United Nations Educational, Scientific and Cultural Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNESCO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNESCO United Nations Educational, Scientific and Cultural Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1260', 'UPU Universal Postal Union', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UPU', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UPU Universal Postal Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1246', 'ICAO International Civil Aviation Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICAO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICAO International Civil Aviation Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1262', 'WIPO World Intellectual Property Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WIPO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WIPO World Intellectual Property Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1247', 'IFAD International Fund for Agricultural Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IFAD', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IFAD International Fund for Agricultural Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1244', 'FAO Food and Agriculture Organization of the United Nations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FAO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FAO Food and Agriculture Organization of the United Nations')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1249', 'IMO International Maritime Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IMO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IMO International Maritime Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1264', 'UNWTO World Tourism Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNWTO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNWTO World Tourism Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1232', 'PAHO Pan American Health Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PAHO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WHO / PAHO World Health Organization incl. PAHO')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1248', 'ILO International Labour Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ILO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ILO International Labour Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1251', 'ITU International Telecommunication Union', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ITU', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ITU International Telecommunication Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1263', 'WMO World Meteorological Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WMO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WMO World Meteorological Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1261', 'WHO World Health Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WHO', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WHO / PAHO World Health Organization incl. PAHO')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1682', 'CPI Community Partners International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CPI', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CPI Community Partners International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1582', 'INS-NGO (International)', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INS-NGO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'INS-NGO (International)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1007', 'Assist International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ASSIST', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Assist International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1021', 'CORDAID Catholic Organisation for Relief and Development Aid', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CORDAID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CORDAID Catholic Organisation for Relief and Development Aid')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1035', 'FHI 360', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FHI 360', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FHI 360')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1776', 'African Society for Laboratory Medicine', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ASLM', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'African Society for Laboratory Medicine')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1791', 'Alter Vida', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Alter Vida', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Alter Vida')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1072', 'OSISA Open Society Initiative for Southern Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OSISA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OSISA Open Society Initiative for Southern Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1713', 'Malaria No More', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Malaria No More', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Malaria No More')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1700', 'Digital Good', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Digital Good', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Digital Good')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1809', 'Windward Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Windward Fund', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Windward Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1033', 'Sheikh Eid Bin Mohammed Al Thani Charity Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Eid Charity', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sheikh Eid Bin Mohammed Al Thani Charity Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1697', 'Romanian Angel Appeal', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Romanian Angel Appeal', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Romanian Angel Appeal')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1644', 'WALIC West Africa Livestock Innovation Centre', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WALIC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WALIC West Africa Livestock Innovation Centre')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1173', 'Association for a UN Live Museum', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN Live Museum', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Association for a UN Live Museum')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1800', 'CAMEG Centrale d''achat des médicaments essentiels génériques et des consommables médicaux', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CAMEG', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CAMEG Centrale d''achat des médicaments essentiels génériques et des consommables')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1055', 'KARCPP King Abdullah Relief Campaign for the Pakistani People', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KARCPP', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'KARCPP King Abdullah Relief Campaign for the Pakistani People')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1766', 'GCA Global Centre on Adaptation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GCA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GCA Global Centre on Adaptation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1573', 'INT-NGO International Non-Governmental Organization', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'INT-NGO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'INT-NGO International Non-Governmental Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1587', 'UNA USA United Nations Association of the USA', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNA-USA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNA USA United Nations Association of the USA')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1796', 'JSI Research and Training Institute, Inc.', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JSI Institute', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JSI Research and Training Institute, Inc.')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1659', 'Comic Relief', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Comic Relief', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Comic Relief')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1658', 'MFM Menschen für Menschen', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MFM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MFM Menschen für Menschen')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1740', 'PATH', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PATH', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PATH')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1152', 'Silatech', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Silatech', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Silatech')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1160', 'Tearfund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Tearfund', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Tearfund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1665', 'ClimateWorks Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ClimateWorks Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ClimateWorks Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1808', 'THPS Tanzania Health Promotion Support', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'THPS', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'THPS Tanzania Health Promotion Support')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1736', 'TMEA TradeMark East Africa', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TMEA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'TMEA TradeMark East Africa')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1159', 'TDH Terre des Hommes Italy', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TDH Italy', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'TDH Terre des Hommes Italy')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1666', 'CRS Catholic Relief Services', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Catholic Relief Services', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CRS Catholic Relief Services')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1013', 'CBHF Clinton Bush Haiti Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CBHF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CBHF Clinton Bush Haiti Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1794', 'One Earth', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'One Earth', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'One Earth')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1094', 'IsraAID Israel Forum for  International Humanitarian Aid', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IsraAID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IsraAID Israel Forum for International Humanitarian Aid')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1663', 'Association IPE', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Association IPE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Association IPE')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1797', 'LDSC Later Day Saints Charities', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'LDSC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'LDSC Later Day Saints Charities')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1454', 'World Vision', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'World Vision', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'World Vision')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1655', 'MFSL Médecins Sans Frontières Logistics', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MSFL', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MFSL Médecins Sans Frontières Logistics')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1005', 'AmeriCares Foundation', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AmeriCares Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AmeriCares Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1789', 'Center for Health Policies and Studies PAS Center', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PAS Center', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Center for Health Policies and Studies PAS Center')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1053', 'IRW Islamic Relief Worldwide', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IRW', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IRW Islamic Relief Worldwide')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1731', 'KNCV Koninklijke Nederlandse Centrale Vereniging tot bestrijding der Tuberculose', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'KNCV', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'KNCV Koninklijke Nederlandse Centrale Vereniging tot bestrijding der Tuberculose')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1721', 'PSI Population Services International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PSI', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PSI Population Services International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1584', 'OBR-NGO (National)', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OBR-NGO (National)', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OBR-NGO (National)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1747', 'AFC Asian Football Confederation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AFC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AFC Asian Football Confederation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1453', 'UMCOR United Methodist Committee on Relief', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UMCOR', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UMCOR United Methodist Committee on Relief')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1661', 'GCDP Global Commission on Drug Policy', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GCDP', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GCDP Global Commission on Drug Policy')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1673', 'Devnet International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Devnet International', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Devnet International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1451', 'Millennium Promise', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Millennium Promise', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Millennium Promise')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1450', 'American Red Cross', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'American Red Cross', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'American Red Cross')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1701', 'PBSP Philippine Business for Social Progress', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PBSP', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PBSP Philippine Business for Social Progress')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1744', 'National Geographic Society', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'National Geographic Society', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'National Geographic Society')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1052', 'Interpeace', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Interpeace', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Interpeace')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1050', 'IFA International Fertilizer Industry Association', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IFA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IFA International Fertilizer Industry Association')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1012', 'Caritas Internationalis', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Caritas Internationalis', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Caritas Internationalis')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1699', 'Sustainable Markets Foundation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Sustainable Markets Foundation', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sustainable Markets Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1814', 'SES Socios en Salud Sucursal Peru', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SES Peru', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SES Socios en Salud Sucursal Peru')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1037', 'FPN Fundacion Patagonia Natural', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'FPN', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'FPN Fundacion Patagonia Natural')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1668', 'Soins de santé primaires en milieu rural (SANRU)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SANRU', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Soins de santé primaires en milieu rural (SANRU)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1569', 'Hammer Forum', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Hammer Forum', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Hammer Forum')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1574', 'NAT-NGO Non-Governmental Organization', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NAT-NGO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'NAT-NGO Non-Governmental Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1771', 'CHAG Christian Health Association of Ghana', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CHAG', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CHAG Christian Health Association of Ghana')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1452', 'Save the Children', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Save the Children', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Save the Children')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1825', 'IFRC International Federation of Red Cross and Red Crescent Societies', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IFRC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IFRC International Federation of Red Cross and Red Crescent Societies')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1714', 'QRCS Qatar Red Crescent Society', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QRCS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'QRCS Qatar Red Crescent Society')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1627', 'IRC International Rescue Committee', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IRC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IRC International Rescue Committee')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1626', 'OXFAM International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Oxfam', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OXFAM International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1647', 'Woord en Daad', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Woord en Daad', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Woord en Daad')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1824', 'Amref Health Africa in Kenya', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Amref Health', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Amref Health Africa in Kenya')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1583', 'OBR-NGO (International)', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OBR-NGO (International)', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OBR-NGO (International)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1155', 'SSACONG Congregation of the Sisters of Saint Anne', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SSACONG', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SSACONG Congregation of the Sisters of Saint Anne')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1667', 'RBM Roll Back Malaria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'RBM', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'RBM Roll Back Malaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1601', 'IUCN International Union for Conservation of Nature', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IUCN', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IUCN International Union for Conservation of Nature')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1047', 'IATI-TF International Aid Transparency Initiative Trust Fund', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IATI Trust Fund', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IATI-TF International Aid Transparency Initiative Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1686', 'The Defeat-NCD Partnership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Defeat-NCD Partnership', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'The Defeat-NCD Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1157', 'Stop TB Partnership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Stop TB Partnership', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Stop TB Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1672', 'SUN Scaling Up Nutrition Movement', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SUN', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SUN Scaling Up Nutrition Movement')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1040', 'Global Alliance for Clean Cookstoves', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Global Alliance for Clean Cookstoves', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Global Alliance for Clean Cookstoves')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1662', 'GHL Global Humanitarian Lab', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GHL', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GHL Global Humanitarian Lab')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1046', 'IATI International Aid Transparency Initiative', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IATI', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IATI International Aid Transparency Initiative')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1820', 'CoNISMa Consorzio Nazionale Interuniversitario per le Scienze del Mare', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CoNISMa', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CoNISMa Consorzio Nazionale Interuniversitario per le Scienze del Mare')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1694', 'DIIS Danish Institute for International Studies', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Danish Institute for International Studi', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DIIS Danish Institute for International Studies')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1804', 'ITRC International Tuberculosis Research Center', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ITRC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ITRC International Tuberculosis Research Center')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1692', 'University of Oxford', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'University of Oxford', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'University of Oxford')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1597', 'IFPRI International Food Policy Research Institute', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IFPRI', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IFPRI International Food Policy Research Institute')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1676', 'SwedBio Swedish International Biodiversity Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SwedBio', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SwedBio Swedish International Biodiversity Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1709', 'Columbia University', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Columbia University', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Columbia University')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1821', 'University of Genova', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'University of Genova', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'University of Genova')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1695', 'University of Notre Dame', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'University of Notre Dame', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'University of Notre Dame')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1640', 'UPNFM National Pedagogical University Francisco Morazan', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UPNFM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UPNFM National Pedagogical University Francisco Morazan')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1830', 'LSTM Liverpool School of Tropical Medicine', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'LSTM', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'LSTM Liverpool School of Tropical Medicine')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1048', 'ICARDA International Center for Agricultural Research in the Dry Areas', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICARDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICARDA International Center for Agricultural Research in the Dry Areas')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1719', 'GELI Global Executive Leadership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GELI', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GELI Global Executive Leadership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1019', 'CLAEH Latin American Centre for Human Economy', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CLAEH', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CLAEH Latin American Centre for Human Economy')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1003', 'ACFE Association of Certified Fraud Examiners', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ACFE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ACFE Association of Certified Fraud Examiners')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1443', 'IBRD International Bank of Reconstruction and Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IBRD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1547', 'IFC International Finance Corporation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IFC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1444', 'IDA International Development Association', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IDA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1546', 'MIGA Multilateral Investment Guarantee Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MIGA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1646', 'The World Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1628', 'ICSID International Centre for Settlement of Investment Disputes', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICSID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WBG World Bank Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1817', 'OFID OPEC Fund for International Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OFID', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OFID OPEC Fund for International Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1011', 'CAF Development Bank of Latin America', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CAF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CAF Development Bank of Latin America')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1442', 'IADB Inter-American Development Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IADB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IADB Inter-American Development Bank')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1568', 'BCIE Central American Bank for Economic Integration', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'BCIE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'BCIE Central American Bank for Economic Integration')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1571', 'IsDB Islamic Development Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IsDB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IsDB Islamic Development Bank')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1439', 'CDB Caribbean Development Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CDB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CDB Caribbean Development Bank')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1437', 'AfDB African Development Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AFDB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AfDB African Development Bank')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1438', 'ADB Asian Development Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ADB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ADB Asian Development Bank')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1441', 'EBRD European Bank for Reconstruction and Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EBRD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EBRD European Bank for Reconstruction and Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1250', 'IMF International Monetary Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IMF', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IMF International Monetary Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1572', 'AFESD Arab Fund for Economic and Social Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AFESD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AFESD Arab Fund for Economic and Social Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1440', 'CFC Common Fund for Commodities', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CFC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3d) International Financial Institution', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CFC Common Fund for Commodities')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1459', 'EIF-1 Enhanced Integrated Framework Phase 1', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EIF Phase 1', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EIF Enhanced Integrated Framework')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1654', 'EIF-2 Enhanced Integrated Framework Phase 2', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EIF Phase 2', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EIF Enhanced Integrated Framework')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1018', 'Cities Alliance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Cities Alliance', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Cities Alliance')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1045', 'GPE Global Partnership for Education', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GPE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GPE Global Partnership for Education')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1054', 'SEforALL Sustainable Energy for All', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SEforALL', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SEforALL Sustainable Energy for All')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1060', 'Nutrition International', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nutrition International', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nutrition International')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1068', 'Nansen Initiative', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nansen Initiative', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nansen Initiative')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1150', 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ARISE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1156', 'Somalia Stability Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SSF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Somalia Stability Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1172', 'UNITAID International Drug Purchase Facility', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNITAID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNITAID International Drug Purchase Facility')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1242', 'WSSCC Water Supply and Sanitation Collaborative Council', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'WSSCC', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WSSCC Water Supply and Sanitation Collaborative Council')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1445', 'GAVI Global Alliance for Vaccination and Immunization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GAVI', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GAVI Global Alliance for Vaccination and Immunization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1447', 'GEF Global Environment Facility', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GEF', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a / 4) Vertical Fund / Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GEF Global Environment Facility')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1458', '3DF Three Disease Fund - EDIT BY SYS ADMIN Test with Lars again', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '3DF', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3DF Three Disease Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1460', 'LIFT Livelihoods and Food Security Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'LIFT', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'LIFT Livelihoods and Food Security Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1462', 'PONREPP-TF Post-Nargis Response and Preparedness Plan Trust Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PONREPP-TF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PONREPP-TF Post-Nargis Response and Preparedness Plan Trust Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1592', 'DAG Development Assistance Group', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DAG', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3b) Funds from UN entity', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DAG Development Assistance Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1606', 'AF Adaptation Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Adaptation Fund', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AF Adaptation Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1607', 'Montreal Protocol', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Montreal Protocol', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Montreal Protocol')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1650', 'JPF Joint Peace Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'JPF', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JPF Joint Peace Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1683', 'ICMPD International Centre for Migration Policy Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICMPD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICMPD International Centre for Migration Policy Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1698', 'JPP Somalia Joint Police Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Somalia JPP', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'JPP Somalia Joint Police Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1717', 'ICAT Initiative for Climate Action Transparency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICAT', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICAT Initiative for Climate Action Transparency')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1729', 'Energy Transition Partnership', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ETP', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Energy Transition Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1730', 'Peace Process Support - The Secretariat', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Peace Process Support Secretariat', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Peace Process Support - The Secretariat')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1756', 'The Climate Vulnerable Forum & Vulnerable Twenty Group of Ministers of Finance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CVF/V20', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'The Climate Vulnerable Forum & Vulnerable Twenty Group of Ministers of Finance')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1763', 'Joint Support to Somaliland National Electoral Commission', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Joint Support to Somaliland NEC', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Joint Support to Somaliland National Electoral Commission')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1774', 'SHF Sanitation and Hygiene Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SHF', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SHF Sanitation and Hygiene Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1778', 'CMI Center for Mediterranean Integration', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CMI', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CMI Center for Mediterranean Integration')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1782', 'ATscale, the Global Partnership for Assistive Technology', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ATScale', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ATscale, the Global Partnership for Assistive Technology')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1790', 'NDC Partnership Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NDCP', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'NDC Partnership Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1448', 'GFATM Global Fund to Fight AIDS, Tuberculosis and Malaria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GFATM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GFATM Global Fund to Fight Aids, Tuberculosis and Malaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1679', 'GFATM-AID Global Fund to fight AIDS', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GFATM-AID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GFATM Global Fund to Fight Aids, Tuberculosis and Malaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1680', 'GFATM-TUB Global Fund to fight Tuberculosis', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GFATM-MAL', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GFATM Global Fund to Fight Aids, Tuberculosis and Malaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1681', 'GFATM-MAL Global Fund to fight Malaria', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GFATM-TUB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3a) Vertical Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GFATM Global Fund to Fight Aids, Tuberculosis and Malaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1687', 'Myanmar Access to Health', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Myanmar Access to Health', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3MDG/Myanmar Access for Health')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1001', '3MDG Three Millennium Development Goal Fund - edit by admin', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '3MDG', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'WHO / PAHO World Health Organization incl. PAHO')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1648', 'AU African Union', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'African Union', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AU African Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1006', 'AMISOM African Union Mission in Somalia', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AMISOM', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AU African Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1165', 'UNAMID African Union-United Nations Hybrid Operation in Darfur', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAMID', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AU African Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1434', 'AU African Union', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'African Union', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AU African Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1739', 'IcSP Instrument contributing to Stability and Peace', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IcSP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1026', 'DG NEAR Directorate-General for Neighbourhood and Enlargement Negotiations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EC DG NEAR', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1032', 'EIB European Investment Bank', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EIB', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1031', 'EEAS European External Action Service', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EEAS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1029', 'DG ECHO Directorate-General for European Civil Protection and Humanitarian Aid Operations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EC DG ECHO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1649', 'EC European Commission (other)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EC - Other', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1025', 'DG INTPA Directorate-General for International Partnerships (formerly DG DEVCO)', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EC DG INTPA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Funding source administers directly (no changes required to the partner agreement)', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EU European Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1004', 'ARC African Risk Capacity', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'African Risk Capacity', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARC African Risk Capacity')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1010', 'CAC Central American Agricultural Council', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CAC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CAC Central American Agricultural Council')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1017', 'CILSS Permanent Inter-State Committee on Drought Control in the Sahel', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CILSS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CILSS Permanent Inter-State Committee on Drought Control in the Sahel')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1028', 'UN ECCAS Economic Community of Central African States', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UN ECCAS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UN ECCAS Economic Community of Central African States')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1030', 'ECOWAS Economic Community of West African States', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ECOWAS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ECOWAS Economic Community of West African States')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1039', 'G77 Group of 77', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'G77', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'G77 Group of 77')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1042', 'GFDRR Global Facility for Disaster Reduction and Recovery', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GFDRR', 'Yes', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '4) Pooled Fund', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GFDRR Global Facility for Disaster Reduction and Recovery')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1044', 'GLOBE International Global Legislators Organisation', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'GLOBE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'GLOBE International Global Legislators Organisation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1051', 'IGAD Intergovernmental Authority on Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IGAD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IGAD Intergovernmental Authority on Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1059', 'MERCOSUR Southern Common Market', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'MERCOSUR', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'MERCOSUR Southern Common Market')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1069', 'OECD Organisation for Economic Co-operation and Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OECD', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OECD Organisation for Economic Co-operation and Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1070', 'OIRSA Organismo Internacional Regional De Sanidad Agropecuaria', 'Active', 'Not allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OIRSA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OIRSA Organismo Internacional Regional De Sanidad Agropecuaria')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1161', 'UEMOA West African Economic and Monetary Union', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'UEMOA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UEMOA West African Economic and Monetary Union')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1231', 'IRENA International Renewable Energy Agency', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IRENA', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'IRENA International Renewable Energy Agency')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1253', 'CTBTO Preparatory Commission for the Nuclear-Test-Ban Treaty Organization', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'CTBTO', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CTBTO Preparatory Commission for the Nuclear-Test-Ban Treaty Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1435', 'NBI Nile Basin Initiative', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'NBI', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'NBI Nile Basin Initiative')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1436', 'SADC Southern African Development Community', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SADC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SADC Southern African Development Community')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1449', 'OSCE Organization for Security and Co-operation in Europe', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OSCE', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OSCE Organization for Security and Co-operation in Europe')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1577', 'OECS Organisation of Eastern Caribbean States', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OECS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OECS Organisation of Eastern Caribbean States')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1578', 'OPEC Organization of the Petroleum Exporting Countries', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OPEC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OPEC Organization of the Petroleum Exporting Countries')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1591', 'Nordic Development Fund', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Nordic Development Fund', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Nordic Development Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1603', 'PIFS Pacific Islands Forum Secretariat', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'PIFS', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'PIFS Pacific Islands Forum Secretariat')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1604', 'OIF Organisation internationale de la Francophonie', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'OIF', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'OIF Organisation internationale de la Francophonie')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1653', 'Office of the Quartet', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Office of the Quartet', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Office of the Quartet')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1660', 'ICC International Criminal Court', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ICC', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ICC International Criminal Court')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1674', 'Itaipu Binacional', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Itaipu Binacional', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', '3c) Programme Country', NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Itaipu Binacional')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1675', 'G5 Sahel Group of Five for the Sahel', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'G5 Sahel', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'G5 Sahel Group of Five for the Sahel')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1689', 'RSHQ Resolute Support HQ – NATO', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'RSHQ', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'RSHQ Resolute Support HQ – NATO')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1704', 'C40 Climate Leadership Group', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'C40', 'No', 'Yes', 'No', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'C40 Climate Leadership Group')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1792', 'ISA International Solar Alliance', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ISA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ISA International Solar Alliance')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1802', 'ECSAHC East, Central, and Southern Africa Health Community', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ECSAHC', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ECSAHC East, Central, and Southern Africa Health Community')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1806', 'SACEP South Asia Cooperative Environment Programme', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SACEP', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'SACEP South Asia Cooperative Environment Programme')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1807', 'EBY Entidad Binacional Yacyretá', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'EBY', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'EBY Entidad Binacional Yacyretá')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1811', 'ASEAN Association of Southeast Asian Nations', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'ASEAN', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ASEAN Association of Southeast Asian Nations')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1826', 'AGFUND Arab Gulf Fund for Development', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'AGFUND', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially applies', NULL, 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'AGFUND Arab Gulf Fund for Development')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '8002', 'Test Partner Account', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TPA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3DF Three Disease Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1831', 'Test Partner Account', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TPA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3DF Three Disease Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '10000', 'Test Partner Account', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'TPA', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3DF Three Disease Fund')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1832', 'QA ADMIN PROSPECT ACCOUNT E2E - AUTOMATED - NOW PARTNER', 'Active', 'Allowed', '123456789', NULL, 'STREET', 'CITY', 'STATE', 'CODE', 'COUNTRY', 'QA_Partner', 'Yes', 'Yes', 'Yes', 'EAC REF', 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ECEAP Estonian Center for Eastern Partnership')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1834', 'ADM - BRANCH ACC KOREA - E2E - now partner', 'Active', 'Allowed', '23213123', 'WWW.GOOGLE.COM', NULL, NULL, NULL, NULL, NULL, 'QA_Partner', 'No', 'Yes', 'Yes', 'EAC REF', 'FALSE', 'FALSE', 'Potentially does not apply', '3a) Vertical Fund', 'Please consult funding source', 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1833', 'ADM - PARTNER ACC - E2E', 'Active', 'Allowed', '12343224', 'WWW.GOOGLE.COM', 'Soi Witthayu 1', NULL, NULL, '10330', 'Thailand', 'QA', 'Yes', 'Yes', 'Yes', 'EAC', 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = '3MDG/Myanmar Access for Health')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1834', 'ADM - PROSPECT ACC - E2E - GMAIL EXT - NOW PARTNER', 'Active', 'Allowed', '3221421', 'WWW.GOOGLE.COM', 'Jaipur Golden Hospital Road', 'Delhi', 'DL', '110085', 'India', 'QA_Partner', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ADM - PRTNR CTGRY')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1835', 'Testing with Lars', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Testing with Lars', 'No', 'No', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Albania')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1836', 'ADM - PARTNER ACC', 'Inactive', 'Allowed', '23331', 'WWW.GOOGLE.COM', 'Princes Park, Tilak Marg, Delhi High Court, India Gate, New Delhi, Delhi Division', 'New Delhi', 'Delhi', '110001', 'IN', 'TEST01', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'South Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1837', 'ADM - PARTNR ACC - PARTNR DONOR CODE', 'Active', 'Allowed', '122132344', 'WWW.GOOGLE.COM', NULL, NULL, NULL, NULL, NULL, 'TEST01', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Democratic People''s Republic of Korea')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1841', 'Test Prospect account', 'Inactive', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'now partner', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'CTBTO Preparatory Commission for the Nuclear-Test-Ban Treaty Organization')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1840', 'QA Partner Acc', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QPA', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'France')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1838', 'ADM - PRTNR ACCNT - E2E', 'Active', 'Allowed', '34434243', 'w.gool123', 'Princes Park, Tilak Marg, Delhi High Court, India Gate, New Delhi, Delhi Division', 'New Delhi', 'Delhi', '110001', 'IN', 'SHORT001', 'Yes', 'Yes', 'Yes', 'EAC', 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Sweden')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1839', 'Testing OUP-4582', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '4582', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DC QA')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1842', 'BRANCH ACCNT TO PRTNR - E2E AUTO C3pTdni', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA jyF', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1843', 'DC QA Partner Account', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'DCQA', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'DC QA')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1849', 'BRANCH ACCNT TO PRTNR - E2E AUTO w9XPNbV', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA Bkh', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1851', 'Branch2Partner Account - Integration Test', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'B2P', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Chinese Red Cross Foundation')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1848', 'E2E Prospect2Partner - AUTO YXTjdbo', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA Qoz', 'Yes', 'No', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1845', 'E2E Prospect2Partner - AUTO EThfErH', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA aM3', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1844', 'E2E Partner Account - automated VITvJjt', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QA WO7E', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1847', 'E2E Partner Account - EDITED', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QA UqFo', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1846', 'BRANCH ACCNT TO PRTNR - E2E AUTO IFrc5W5', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA hSr', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1850', 'Integration Test Partner Account', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'SIT TEST', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'Thailand')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1853', 'E2E Partner Account - automated FpXb18R', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QA HuMu', 'Yes', 'No', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1852', 'Prospect2Partner - Integration test', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'P2P', 'No', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'University of Oxford')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1854', 'E2E Partner Account - EDITED', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'QA nNIb', 'Yes', 'Yes', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'ARISE Private Sector Alliance for Disaster Resilient Societies (formerly R!SE)')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1856', 'BRANCH ACCNT TO PRTNR - E2E AUTO BtLl3Kc', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA IHk', 'Yes', 'No', 'Yes', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '1855', 'E2E Prospect2Partner - AUTO dIgVIMs', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'E2E QA fwC', 'Yes', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Potentially does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UK United Kingdom')
);

INSERT INTO public."Partners" (
    "PartnerCode", "Name", "Status", "NewEngagement", "Phone", "Website", 
    "Address1Street", "Address1City", "Address1StateProvince", "Address1PostalCode", "Address1Country",
    "ShortName", "PooledFund", "DDRequired", "DDEACDone",
    "EACReference", "GlobalKeyAccount", "UNSecretariatEntity", "LevyPotentiallyApplies", "ReasonForLevyNotApplying",
    "LevyTreatment", "Discriminator", "CreatedBy", "CreatedDate", "LastModifiedBy", "IsDeleted", "DeletedBy", "PartnerCategoryId"
) VALUES (
    '9012', 'IPSAS Accounting', 'Active', 'Allowed', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'IPSAS Accounting', 'No', 'No', 'No', NULL, 'FALSE', 'FALSE', 'Does not apply', NULL, NULL, 'UNOPSPartner', '0', 'NOW()', '0', 'FALSE', '0',
    (SELECT category_id FROM temp_partner_categories WHERE category_name = 'UNOPS United Nations Office for Project Services')
);

-- Clean up temporary tables
DROP TABLE temp_partner_categories;

COMMIT;