export interface ResponsiveTabItem {
  label: string;
  route: string;
  translatedLabel?: string;
  icon?: string;
  disabled?: boolean;
}

export interface ResponsiveTabsConfig {
  dropdownPlaceholder?: string;
  tabsClass?: string;
  tabListClass?: string;
  activeTabClass?: string;
  inactiveTabClass?: string;
  breakpoint?: number;
  disabled?: boolean;
}
