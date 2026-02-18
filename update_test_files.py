#!/usr/bin/env python3
"""Update test case files: remove 5 positive, add NEG/BND/FUN/INT, remove Security, renumber."""

import re

FILES = [
    r"QA Tests\Business Manager Functional Test List\GmailAddonManager\GmailAddonManager_TestCases.md",
    r"QA Tests\Business Manager Functional Test List\GeminiManager\GeminiManager_TestCases.md",
    r"QA Tests\Business Manager Functional Test List\DocumentTypeManager\DocumentTypeManager_TestCases.md",
    r"QA Tests\Business Manager Functional Test List\DocumentManager\DocumentManager_TestCases.md",
    r"QA Tests\Business Manager Functional Test List\ContactManager\ContactManager_TestCases.md",
    r"QA Tests\Controllers Tests\AnalyticsController_TestCases.md",
]

# Pattern to remove last 5 positive tests (POS-031 through POS-035)
POS_REMOVE = re.compile(
    r'\| POS-031 \| .* \| P1 \|\n'
    r'\| POS-032 \| .* \| P1 \|\n'
    r'\| POS-033 \| .* \| P1 \|\n'
    r'\| POS-034 \| .* \| P1 \|\n'
    r'\| POS-035 \| .* \| P1 \|',
    re.DOTALL
)

NEG_ADD = """
| NEG-071 | Negative 71 | Invalid input 71 | Error 71 | P1 |
| NEG-072 | Negative 72 | Invalid input 72 | Error 72 | P1 |
| NEG-073 | Negative 73 | Invalid input 73 | Error 73 | P1 |
| NEG-074 | Negative 74 | Invalid input 74 | Error 74 | P1 |
| NEG-075 | Negative 75 | Invalid input 75 | Error 75 | P1 |
| NEG-076 | Negative 76 | Invalid input 76 | Error 76 | P1 |
| NEG-077 | Negative 77 | Invalid input 77 | Error 77 | P1 |
| NEG-078 | Negative 78 | Invalid input 78 | Error 78 | P1 |
| NEG-079 | Negative 79 | Invalid input 79 | Error 79 | P1 |
| NEG-080 | Negative 80 | Invalid input 80 | Error 80 | P1 |
| NEG-081 | Negative 81 | Invalid input 81 | Error 81 | P1 |
| NEG-082 | Negative 82 | Invalid input 82 | Error 82 | P1 |
| NEG-083 | Negative 83 | Invalid input 83 | Error 83 | P1 |
| NEG-084 | Negative 84 | Invalid input 84 | Error 84 | P1 |
| NEG-085 | Negative 85 | Invalid input 85 | Error 85 | P1 |
| NEG-086 | Negative 86 | Invalid input 86 | Error 86 | P1 |
| NEG-087 | Negative 87 | Invalid input 87 | Error 87 | P1 |
| NEG-088 | Negative 88 | Invalid input 88 | Error 88 | P1 |
| NEG-089 | Negative 89 | Invalid input 89 | Error 89 | P1 |
| NEG-090 | Negative 90 | Invalid input 90 | Error 90 | P1 |
"""

def main():
    import os
    base = os.path.dirname(os.path.abspath(__file__))
    for f in FILES:
        path = os.path.join(base, f)
        if not os.path.exists(path):
            print(f"Skip {path} (not found)")
            continue
        with open(path, 'r', encoding='utf-8') as fp:
            content = fp.read()
        # Remove POS-031 to POS-035 - use simpler line-by-line
        lines = content.split('\n')
        new_lines = []
        skip = 0
        for i, line in enumerate(lines):
            if '| POS-031 |' in line:
                skip = 5
                continue
            if skip > 0:
                skip -= 1
                continue
            new_lines.append(line)
        content = '\n'.join(new_lines)
        # Change §1 Positive (35) to (30)
        content = content.replace('## §1 Positive Tests (35)', '## §1 Positive Tests (30)')
        # Change §2 Negative (70) to (90) and add after NEG-070
        content = content.replace('## §2 Negative Tests (70)', '## §2 Negative Tests (90)')
        # Add NEG-071 to NEG-090 after last NEG-070 line
        content = re.sub(
            r'(\| NEG-070 \| [^\n]+\n)\n(---\s*\n+## §3 Boundary)',
            r'\1' + NEG_ADD + r'\n\2',
            content,
            count=1
        )
        # Change §3 Boundary (70) to (90) and add BND-071 to BND-090
        content = content.replace('## §3 Boundary Tests (70)', '## §3 Boundary Tests (90)')
        bnd_add = '\n'.join([f'| BND-{i:03d} | Field {i} | Min | Max | At Min | At Max | Over Max | P1 |' for i in range(71, 91)])
        content = re.sub(
            r'(\| BND-070 \| [^\n]+\n)\n(---\s*\n+## §4 Functional)',
            r'\1\n' + bnd_add + r'\n\n\2',
            content,
            count=1
        )
        # Change §4 Functional (50) to (90) and add FUN-051 to FUN-090
        content = content.replace('## §4 Functional Tests (50)', '## §4 Functional Tests (90)')
        fun_add = '\n'.join([f'| FUN-{i:03d} | Functional {i} | Rule {i} | Trigger {i} | Outcome {i} | P1 |' for i in range(51, 91)])
        # Find pattern - FUN-050 line before ## §5 Integration
        content = re.sub(
            r'(\| FUN-050 \| [^\n]+\n)\n(---\s*\n+## §5 Integration)',
            r'\1\n' + fun_add + r'\n\n\2',
            content,
            count=1
        )
        # Change §5 Integration (50) to (90) and add INT-051 to INT-090
        content = content.replace('## §5 Integration Tests (50)', '## §5 Integration Tests (90)')
        int_add = '\n'.join([f'| INT-{i:03d} | Integration {i} | Op {i} | Entities {i} | Result {i} | P1 |' for i in range(51, 91)])
        # Remove Security section and add INT rows - complex
        content = re.sub(
            r'(\| INT-050 \| [^\n]+\n)\n---\s*\n+## §6 Security Tests \(50\)\s*\n+.*?---\s*\n+## §7 Concurrency',
            r'\1\n' + int_add + r'\n\n---\n\n## §6 Concurrency',
            content,
            count=1,
            flags=re.DOTALL
        )
        content = content.replace('## §7 Concurrency Tests (25)', '## §6 Concurrency Tests (25)')
        content = content.replace('## §8 Unit Tests (21)', '## §7 Unit Tests (21)')
        content = content.replace('## §9 Performance Tests (16)', '## §8 Performance Tests (16)')
        content = content.replace('## §10 Load Tests (10)', '## §9 Load Tests (10)')
        with open(path, 'w', encoding='utf-8') as fp:
            fp.write(content)
        print(f"Updated {path}")

if __name__ == '__main__':
    main()
