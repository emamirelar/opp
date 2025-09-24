const fs = require('fs');
const path = require('path');

// Chemins des fichiers de traduction
const i18nDir = './public/assets/i18n';
const languages = ['en', 'fr', 'pt', 'span'];

// Charger tous les fichiers de traduction
const translations = {};
for (const lang of languages) {
  const filePath = path.join(i18nDir, `${lang}.json`);
  if (fs.existsSync(filePath)) {
    const content = fs.readFileSync(filePath, 'utf8');
    translations[lang] = JSON.parse(content);
  } else {
    translations[lang] = {};
  }
}

// Extraire toutes les clés uniques de tous les fichiers
const allKeys = new Set();
for (const lang of languages) {
  Object.keys(translations[lang]).forEach(key => allKeys.add(key));
}

console.log(`Total unique keys found: ${allKeys.size}`);

// Synchroniser toutes les clés dans tous les fichiers
let keysAdded = 0;
for (const lang of languages) {
  const originalKeyCount = Object.keys(translations[lang]).length;
  
  for (const key of allKeys) {
    if (!translations[lang].hasOwnProperty(key)) {
      // Laisser vide si la clé n'existe pas
      const placeholder = "";
      translations[lang][key] = placeholder;
      keysAdded++;
    }
  }
  
  // Trier les clés alphabétiquement
  const sortedTranslations = {};
  Object.keys(translations[lang]).sort().forEach(key => {
    sortedTranslations[key] = translations[lang][key];
  });
  
  // Sauvegarder le fichier mis à jour
  const filePath = path.join(i18nDir, `${lang}.json`);
  fs.writeFileSync(filePath, JSON.stringify(sortedTranslations, null, '\t'));
  
  const newKeyCount = Object.keys(sortedTranslations).length;
  console.log(`${lang}.json: ${originalKeyCount} -> ${newKeyCount} keys (${newKeyCount - originalKeyCount} added)`);
}

console.log(`\nTotal keys added across all files: ${keysAdded}`);
console.log(`All translation files now synchronized with ${allKeys.size} keys each.`);