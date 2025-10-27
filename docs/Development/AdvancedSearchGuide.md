# Guide d'utilisation de l'Advanced Search pour les Contacts

## Vue d'ensemble

L'Advanced Search permet aux utilisateurs de créer des requêtes complexes avec des critères multiples, des opérateurs de comparaison et des opérateurs logiques (AND/OR) pour rechercher des contacts dans le système UNOPS PAO.

## Endpoint unifié

### Endpoint principal
```
GET /api/contact?advancedSearch=true&searchCriteria=[JSON]
```

**Avantages de l'approche unifiée :**
- Un seul endpoint à maintenir
- Évolution naturelle de la recherche simple vers l'avancée
- Cohérence avec les autres contrôleurs (Partner, etc.)
- Pas de duplication de code
- Migration transparente pour les clients existants

## Paramètres de requête

### Paramètres pour la recherche textuelle simple
- `searchText` (string) : Recherche textuelle sur tous les champs de l'entité

### Paramètres pour la recherche par filtres spécifiques
- `firstName`, `lastName`, `email`, `status`, `partnerId`, etc. : Filtres directs sur des champs spécifiques

### Paramètres pour l'Advanced Search
- `advancedSearch` (bool) : `true` pour activer la recherche avancée
- `searchCriteria` (string) : JSON sérialisé contenant les critères de recherche

### Paramètres optionnels
- `pageIndex` (int) : Index de la page (défaut: 1)
- `pageSize` (int) : Nombre d'éléments par page (défaut: 20)
- `orderBy` (string) : Champ de tri
- `ascending` (bool) : Direction du tri (true = ASC, false = DESC)

## Modes de fonctionnement

### 1. Recherche textuelle simple (recommandée pour la recherche générale)
```bash
GET /api/contact?searchText=John
```
La recherche textuelle simple utilise le paramètre `searchText` et effectue une recherche sur tous les champs textuels de l'entité Contact, incluant :
- Informations personnelles : `firstName`, `lastName`, `email`, `title`, `department`, `description`
- Informations de contact : `phone`, `mobile`, `assistant`, `assistantEmail`, `assistantPhone`
- Adresse : `mailingCity`, `mailingStateProvince`, `mailingPostalCode`, `mailingCountry`
- Informations du partenaire : `partner.name`, `partner.shortName`, `partner.status`

### 2. Recherche par filtres spécifiques (compatibilité)
```bash
GET /api/contact?firstName=John&status=Active&partnerId=123
```

### 3. Recherche avancée (pour des critères complexes)
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[JSON]
```

### 4. Recherche combinée
```bash
GET /api/contact?searchText=John&pageSize=50&orderBy=lastName&ascending=true
```

## Structure des critères de recherche

### Format JSON
```json
[
  {
    "field": "firstName",
    "value": "John",
    "label": "First Name",
    "operator": "like",
    "logicalOperator": "AND"
  },
  {
    "field": "status",
    "value": "Active",
    "label": "Status",
    "operator": "is",
    "logicalOperator": "OR"
  }
]
```

### Propriétés des critères
- **field** (string) : Nom du champ à rechercher
- **value** (string) : Valeur à rechercher
- **label** (string) : Libellé affiché (pour l'interface utilisateur)
- **operator** (string) : Opérateur de comparaison
- **logicalOperator** (string) : Opérateur logique avec le critère suivant

## Opérateurs supportés

### Opérateurs de comparaison pour les champs texte
- `"is"` : Égalité exacte
- `"is not"` : Inégalité
- `"like"` : Contient (recherche partielle, insensible à la casse)
- `"not like"` : Ne contient pas

### Opérateurs de comparaison pour les champs numériques et dates
- `"is"` : Égalité
- `"is not"` : Inégalité
- `">"` : Supérieur à
- `"<"` : Inférieur à
- `">="` : Supérieur ou égal
- `"<="` : Inférieur ou égal

### Opérateurs logiques
- `"AND"` : ET logique (tous les critères doivent être vrais)
- `"OR"` : OU logique (au moins un critère doit être vrai)

## Champs de recherche autorisés

### Mapping des noms de champs

Le système effectue automatiquement le mapping entre les noms de champs camelCase utilisés par le front-end et les propriétés PascalCase des entités :

| Front-end (camelCase) | Entité (PascalCase) | Description |
|----------------------|---------------------|-------------|
| `firstName` | `FirstName` | Prénom du contact |
| `lastName` | `LastName` | Nom de famille du contact |
| `email` | `Email` | Adresse email |
| `partnerId` | `PartnerId` | ID du partenaire |
| `partner.name` | `Partner.Name` | Nom du partenaire |
| `partner.status` | `Partner.Status` | Statut du partenaire |

### Champs directs du contact
- `id`, `salutation`, `firstName`, `middleName`, `lastName`, `suffix`
- `title`, `department`, `description`, `email`, `phone`, `mobile`
- `assistant`, `assistantPhone`, `assistantEmail`, `status`
- `mailingStreet`, `mailingStreet2`, `mailingCity`, `mailingStateProvince`
- `mailingPostalCode`, `mailingCountry`, `profilePictureUrl`

### Champs liés au partenaire
- `partner.name`, `partner.status`, `partner.shortName`, `partner.phone`
- `partner.website`, `partner.address1City`, `partner.address1Country`
- `partnerId`, `partnerName`, `partnerStatus`, `partnerShortName`

## Exemples d'utilisation

### Recherche textuelle simple

#### 1. Recherche générale par nom
```bash
GET /api/contact?searchText=John
```
Trouve tous les contacts contenant "John" dans n'importe quel champ textuel.

#### 2. Recherche par email
```bash
GET /api/contact?searchText=john@example.com
```
Trouve les contacts avec cet email ou contenant cette chaîne.

#### 3. Recherche par organisation
```bash
GET /api/contact?searchText=UNICEF
```
Trouve tous les contacts liés à UNICEF (dans le nom du partenaire).

#### 4. Recherche avec pagination
```bash
GET /api/contact?searchText=Director&pageIndex=1&pageSize=10&orderBy=lastName&ascending=true
```

### Recherche par filtres spécifiques

#### 1. Filtre par statut
```bash
GET /api/contact?status=Active
```

#### 2. Filtres combinés
```bash
GET /api/contact?firstName=John&status=Active&partnerId=123
```

### Advanced Search (critères complexes)

#### 1. Recherche simple - un critère
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[{"field":"firstName","value":"John","operator":"like"}]
```

#### 2. Recherche multiple avec AND
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[
  {"field":"firstName","value":"John","operator":"like","logicalOperator":"AND"},
  {"field":"status","value":"Active","operator":"is"}
]
```

#### 3. Recherche multiple avec OR
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[
  {"field":"firstName","value":"John","operator":"like","logicalOperator":"OR"},
  {"field":"firstName","value":"Jane","operator":"like"}
]
```

#### 4. Recherche complexe avec AND/OR mixte
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[
  {"field":"firstName","value":"John","operator":"like","logicalOperator":"AND"},
  {"field":"status","value":"Active","operator":"is","logicalOperator":"OR"},
  {"field":"status","value":"Pending","operator":"is"}
]
```

#### 5. Recherche par partenaire
```bash
GET /api/contact?advancedSearch=true&searchCriteria=[
  {"field":"partner.name","value":"UNICEF","operator":"like","logicalOperator":"AND"},
  {"field":"title","value":"Director","operator":"like"}
]
```

#### 6. Recherche avec pagination et tri
```bash
GET /api/contact?advancedSearch=true&pageIndex=1&pageSize=10&orderBy=lastName&ascending=true&searchCriteria=[
  {"field":"status","value":"Active","operator":"is"}
]
```

## Gestion des erreurs

### Erreurs de validation
- **400 Bad Request** : Format JSON invalide
- **400 Bad Request** : Champ non autorisé
- **400 Bad Request** : Opérateur invalide
- **400 Bad Request** : Valeur vide

### Exemple de réponse d'erreur
```json
{
  "error": "Field 'invalidField' is not allowed for search"
}
```

## Sécurité

### Validation des champs
- Seuls les champs autorisés peuvent être utilisés dans les critères de recherche
- La liste des champs autorisés est définie côté serveur et ne peut pas être modifiée par le client

### Validation des opérateurs
- Seuls les opérateurs prédéfinis sont acceptés
- Les opérateurs invalides sont rejetés avec une erreur explicite

### Protection contre l'injection SQL
- Utilisation d'Entity Framework avec des expressions LINQ
- Utilisation d'`EF.Functions.Like` pour les recherches textuelles
- Paramètres liés automatiquement par EF Core

## Performance

### Optimisations implémentées
- Utilisation du pattern Specification pour des requêtes optimisées
- Pagination automatique pour limiter les résultats
- Expressions LINQ compilées pour de meilleures performances

### Recommandations
- Utiliser la pagination pour les grandes collections
- Limiter le nombre de critères de recherche simultanés
- Préférer les recherches exactes (`is`) aux recherches partielles (`like`) quand possible

## Tests

### Tests unitaires disponibles
- Validation du parsing JSON
- Validation des critères de recherche
- Validation des champs autorisés
- Validation des opérateurs
- Tests de sécurité

### Exécution des tests
```bash
dotnet test UNOPS.PAO.Tests --filter "AdvancedSearchTests"
```

## Intégration avec le front-end

L'Advanced Search est conçu pour fonctionner avec le composant Angular `listview-advanced-search` qui :
- Génère automatiquement le JSON des critères
- Valide les entrées utilisateur
- Affiche les critères sous forme de chips
- Gère les opérateurs logiques

Le front-end peut facilement basculer entre les modes :

```typescript
// Mode recherche simple
const simpleParams = {
  searchText: "John",
  status: "Active"
};

// Mode recherche avancée
const advancedParams = {
  advancedSearch: true,
  searchCriteria: JSON.stringify([
    { field: "firstName", value: "John", operator: "like" }
  ])
};
```

## Migration depuis l'ancien système

L'approche unifiée permet une migration en douceur :

1. **Phase 1** : Les clients existants continuent de fonctionner sans modification
2. **Phase 2** : Les nouveaux clients peuvent utiliser l'Advanced Search
3. **Phase 3** : Migration progressive des anciens clients vers l'Advanced Search

Pour plus d'informations sur l'intégration front-end, consultez la documentation du composant Angular correspondant.

## Dépannage

### Problèmes courants

#### 1. Erreur "Field name cannot be empty"
**Cause :** Le JSON des critères de recherche n'est pas correctement parsé.
**Solution :** Vérifiez que le JSON est correctement formaté et URL-encodé.

```json
// ✅ Correct
[{"field":"firstName","value":"John","operator":"like"}]

// ❌ Incorrect - champ vide
[{"field":"","value":"John","operator":"like"}]
```

#### 2. Erreur "Field 'fieldName' is not allowed for search"
**Cause :** Le champ spécifié n'est pas dans la liste des champs autorisés.
**Solution :** Utilisez uniquement les champs listés dans la section "Champs de recherche autorisés".

#### 3. Aucun résultat retourné
**Causes possibles :**
- Nom de champ incorrect (vérifiez le mapping camelCase/PascalCase)
- Valeur de recherche trop restrictive
- Opérateur incorrect pour le type de données

**Solutions :**
- Vérifiez que le nom du champ correspond à ceux autorisés
- Utilisez l'opérateur `like` pour les recherches textuelles
- Testez avec des valeurs plus générales

#### 4. Erreur "Invalid operator"
**Cause :** L'opérateur spécifié n'est pas supporté.
**Solution :** Utilisez uniquement les opérateurs listés dans la documentation :
- Pour les textes : `is`, `is not`, `like`, `not like`
- Pour les nombres/dates : `is`, `is not`, `>`, `<`, `>=`, `<=`

### Exemples de débogage

#### Test d'un champ simple
```bash
# Test avec firstName
GET /api/contact?advancedSearch=true&searchCriteria=[{"field":"firstName","value":"test","operator":"like"}]
```

#### Test avec un champ de partenaire
```bash
# Test avec partner.name
GET /api/contact?advancedSearch=true&searchCriteria=[{"field":"partner.name","value":"UNICEF","operator":"like"}]
```

#### Vérification des logs
Activez les logs de débogage pour voir le traitement des critères :
- Vérifiez que le JSON est correctement décodé
- Vérifiez que les champs sont correctement mappés (camelCase → PascalCase)
- Vérifiez que les expressions LINQ sont correctement générées

### Support technique

Pour obtenir de l'aide supplémentaire :
1. Vérifiez les logs de l'application
2. Testez avec des critères simples avant d'ajouter de la complexité
3. Utilisez les tests unitaires pour valider le comportement
4. Consultez la documentation des spécifications Entity Framework 