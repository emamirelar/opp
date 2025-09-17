#!/usr/bin/env python3
"""
Script to update LiaisonOfficeId in the fixed partner CSV
Updates the mapping to match the correct IDs provided
"""

import csv

def update_liaison_office_mapping():
    """Update liaison office IDs based on the correct mapping"""
    # Current mapping in CSV -> Correct mapping
    id_mapping = {
        '1': '1',   # Washington Liaison Office -> 1 (correct)
        '2': '3',   # Gulf Countries Liaison Office -> 3
        '3': '4',   # Other Partners -> 4  
        '4': '5',   # Northern Europe Liaison Office -> 5
        '5': '6',   # Rome Liaison Office -> 6
        '6': '7',   # Tokyo Liaison Office -> 7
        '7': '8',   # Brussels Liaison Office -> 8
        '8': '9',   # Geneva Liaison Office -> 9
        '9': '10',  # Nairobi Liaison Office -> 10
        '10': '11', # New York Liaison Office -> 11
        '11': '12', # Other PLG Managed Partners -> 12
        '12': '13'  # Manila Liaison Office -> 13
    }
    
    input_file = "partner-full-fixed.csv"
    output_file = "partner-full-fixed-updated.csv"
    
    print(f"Reading from {input_file}")
    print(f"Writing to {output_file}")
    
    try:
        with open(input_file, 'r', newline='', encoding='utf-8') as infile:
            with open(output_file, 'w', newline='', encoding='utf-8') as outfile:
                reader = csv.reader(infile)
                writer = csv.writer(outfile, quoting=csv.QUOTE_MINIMAL)
                
                row_count = 0
                for row in reader:
                    if len(row) > 25:  # Ensure we have the LiaisonOfficeId column
                        # Update LiaisonOfficeId (column 25)
                        current_id = str(row[25])
                        if current_id in id_mapping:
                            row[25] = id_mapping[current_id]
                            print(f"Updated row {row_count + 1}: LiaisonOfficeId {current_id} -> {row[25]}")
                    
                    writer.writerow(row)
                    row_count += 1
                    
                    if row_count % 100 == 0:
                        print(f"Processed {row_count} rows...")
                
                print(f"Completed! Processed {row_count} rows total.")
                print(f"Updated CSV saved as {output_file}")
                
    except FileNotFoundError:
        print(f"Error: Could not find input file {input_file}")
        return False
    except Exception as e:
        print(f"Error: {e}")
        return False
    
    return True

if __name__ == "__main__":
    success = update_liaison_office_mapping()
    if success:
        print("\nLiaisonOffice ID mapping updated successfully!")
        print("Use partner-full-fixed-updated.csv for PostgreSQL import.")
    else:
        print("\nFailed to update LiaisonOffice ID mapping.")
