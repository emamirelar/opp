#!/bin/bash
# UNOPS Opportunity+ Frontend Test Setup Script
# This script copies frontend spec files to the appropriate Angular component/service folders
#
# Usage: ./setup-frontend-tests.sh [--dry-run] [--force]
# Run from: QA Tests/Frontend Tests/ folder OR project root

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Parse arguments
DRY_RUN=false
FORCE=false

for arg in "$@"; do
    case $arg in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --force)
            FORCE=true
            shift
            ;;
    esac
done

# Determine project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo ""
echo -e "${CYAN}========================================"
echo -e "  Frontend Test Setup Script"
echo -e "========================================${NC}"
echo ""
echo -e "Project Root: ${WHITE}$PROJECT_ROOT${NC}"
echo -e "Dry Run: ${WHITE}$DRY_RUN${NC}"
echo ""

# Define mappings (source|destination|name)
declare -a MAPPINGS=(
    "QA Tests/Frontend Tests/components/base-entity-view.component.spec.ts|UNOPS.PAO.ClientApp/src/app/shared/components/base-entity-view|BaseEntityViewComponent"
    "QA Tests/Frontend Tests/components/related-info-panel.component.spec.ts|UNOPS.PAO.ClientApp/src/app/shared/components/related-info-panel|RelatedInfoPanelComponent"
    "QA Tests/Frontend Tests/components/enhanced-entity-layout.component.spec.ts|UNOPS.PAO.ClientApp/src/app/shared/components/enhanced-entity-layout|EnhancedEntityLayoutComponent"
    "QA Tests/Frontend Tests/components/partner-view-enhanced.component.spec.ts|UNOPS.PAO.ClientApp/src/app/features/partnerships/partners/components/partner/view|PartnerViewEnhanced"
    "QA Tests/Frontend Tests/components/contact-view-enhanced.component.spec.ts|UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/view|ContactViewEnhanced"
    "QA Tests/Frontend Tests/services/panel-layout.service.spec.ts|UNOPS.PAO.ClientApp/src/app/shared/services|PanelLayoutService"
)

COPIED=0
SKIPPED=0
CREATED_FOLDERS=0
ERRORS=0

for mapping in "${MAPPINGS[@]}"; do
    IFS='|' read -r SOURCE DEST NAME <<< "$mapping"
    
    SOURCE_PATH="$PROJECT_ROOT/$SOURCE"
    DEST_DIR="$PROJECT_ROOT/$DEST"
    FILENAME=$(basename "$SOURCE")
    DEST_FILE="$DEST_DIR/$FILENAME"
    
    echo -e "${WHITE}Processing: $NAME${NC}"
    
    # Check if source file exists
    if [ ! -f "$SOURCE_PATH" ]; then
        echo -e "  ${YELLOW}[SKIP] Source file not found${NC}"
        ((SKIPPED++))
        continue
    fi
    
    # Create destination folder if it doesn't exist
    if [ ! -d "$DEST_DIR" ]; then
        if [ "$DRY_RUN" = true ]; then
            echo -e "  ${MAGENTA}[DRY RUN] Would create folder: $DEST${NC}"
        else
            mkdir -p "$DEST_DIR"
            echo -e "  ${GREEN}[CREATED] Folder: $DEST${NC}"
            ((CREATED_FOLDERS++))
        fi
    fi
    
    # Check if destination file already exists
    if [ -f "$DEST_FILE" ] && [ "$FORCE" = false ]; then
        echo -e "  ${YELLOW}[SKIP] File already exists (use --force to overwrite)${NC}"
        ((SKIPPED++))
        continue
    fi
    
    # Copy the file
    if [ "$DRY_RUN" = true ]; then
        echo -e "  ${MAGENTA}[DRY RUN] Would copy to: $DEST${NC}"
        ((COPIED++))
    else
        cp "$SOURCE_PATH" "$DEST_FILE"
        echo -e "  ${GREEN}[COPIED] -> $DEST${NC}"
        ((COPIED++))
    fi
done

# Summary
echo ""
echo -e "${CYAN}========================================"
echo -e "  Summary"
echo -e "========================================${NC}"
echo ""
echo -e "  ${GREEN}Copied:          $COPIED files${NC}"
echo -e "  ${YELLOW}Skipped:         $SKIPPED files${NC}"
echo -e "  ${BLUE}Folders Created: $CREATED_FOLDERS${NC}"
echo ""

if [ "$DRY_RUN" = true ]; then
    echo -e "${MAGENTA}This was a DRY RUN. No files were actually copied.${NC}"
    echo -e "${MAGENTA}Run without --dry-run to copy files.${NC}"
    echo ""
fi

# Next steps
echo -e "${CYAN}Next Steps:${NC}"
echo -e "  ${WHITE}1. Navigate to Angular app: cd UNOPS.PAO.ClientApp${NC}"
echo -e "  ${WHITE}2. Install dependencies:    npm install${NC}"
echo -e "  ${WHITE}3. Run tests:               ng test${NC}"
echo ""

