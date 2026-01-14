import csv
import json
from collections import Counter

# Read the JIRA CSV file
with open(r'g:\My Drive\JIRA (2).csv', 'r', encoding='utf-8-sig') as f:  # utf-8-sig to handle BOM
    reader = csv.reader(f)
    headers = next(reader)  # Get headers
    
    defects = []
    for row in reader:
        if len(row) > 5:  # Ensure row has enough columns
            defects.append({
                'summary': row[0].strip(),
                'issue_key': row[1].strip(),
                'issue_id': row[2].strip(),
                'issue_type': row[3].strip(),
                'status': row[4].strip(),
                'priority': row[11].strip() if len(row) > 11 else '',
                'description': row[29][:500] if len(row) > 29 and row[29] else '',  # Description column
                'created': row[19] if len(row) > 19 else '',
                'resolved': row[22] if len(row) > 22 else ''
            })

# Print summary statistics
print("=" * 80)
print("JIRA DEFECTS ANALYSIS")
print("=" * 80)
print(f"\nTotal Issues: {len(defects)}")

# Count by type
types = Counter([d['issue_type'] for d in defects if d['issue_type']])
print("\n--- BY ISSUE TYPE ---")
for issue_type, count in types.most_common():
    print(f"{issue_type:20s}: {count:4d}")

# Count by status
statuses = Counter([d['status'] for d in defects if d['status']])
print("\n--- BY STATUS ---")
for status, count in statuses.most_common():
    print(f"{status:30s}: {count:4d}")

# Count by priority
priorities = Counter([d['priority'] for d in defects if d['priority']])
print("\n--- BY PRIORITY ---")
for priority, count in priorities.most_common():
    print(f"{priority:20s}: {count:4d}")

# Print first 30 bug summaries
bugs = [d for d in defects if d['issue_type'] == 'Bug' and d['summary']]
print(f"\n--- TOP 30 BUG SUMMARIES ---")
for i, bug in enumerate(bugs[:30], 1):
    status_mark = "[X]" if bug['status'] in ['Done', 'Resolved', 'Closed'] else "[ ]"
    summary = bug['summary'][:80].encode('ascii', 'replace').decode('ascii')  # Handle encoding
    print(f"{i:2d}. [{bug['issue_key']}] {status_mark} {summary}")

# Categorize bugs by feature area (simple keyword matching)
categories = {
    'Search/Filter': [],
    'UI/Form': [],
    'Data/Sync': [],
    'AI/Suggestions': [],
    'Opportunity': [],
    'Team/User': [],
    'Document': [],
    'Date/Time': [],
    'Other': []
}

for bug in bugs:
    summary_lower = bug['summary'].lower()
    desc_lower = bug['description'].lower()
    
    categorized = False
    
    if any(kw in summary_lower or kw in desc_lower for kw in ['search', 'filter', 'dropdown']):
        categories['Search/Filter'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['ui', 'form', 'button', 'field', 'input', 'overlap', 'display']):
        categories['UI/Form'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['data', 'sync', 'save', 'load', 'missing']):
        categories['Data/Sync'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['ai', 'suggestion', 'insight', 'recommend']):
        categories['AI/Suggestions'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['opportunity', 'statement', 'concept']):
        categories['Opportunity'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['team', 'user', 'manager', 'stakeholder']):
        categories['Team/User'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['document', 'upload', 'file', 'pdf']):
        categories['Document'].append(bug)
        categorized = True
    if any(kw in summary_lower or kw in desc_lower for kw in ['date', 'time', 'deadline']):
        categories['Date/Time'].append(bug)
        categorized = True
    
    if not categorized:
        categories['Other'].append(bug)

print(f"\n--- BUGS BY CATEGORY ---")
for category, bugs_list in categories.items():
    if bugs_list:
        print(f"\n{category}: {len(bugs_list)} bugs")
        for i, bug in enumerate(bugs_list[:5], 1):
            print(f"  {i}. [{bug['issue_key']}] {bug['summary'][:70]}")
        if len(bugs_list) > 5:
            print(f"  ... and {len(bugs_list) - 5} more")

print("\n" + "=" * 80)
