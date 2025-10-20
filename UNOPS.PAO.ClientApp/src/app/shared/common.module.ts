import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListviewComponent } from './pages/components/listview/listview.component';
import { AdvancedSearchComponent } from './components/advanced-search/advanced-search.component';
import { SearchParserService } from './services/search-parser.service';

// ... other imports ...

@NgModule({
  declarations: [
    ListviewComponent,
    AdvancedSearchComponent,
    // ... other components ...
  ],
  imports: [
    CommonModule,
    FormsModule,
    // ... other imports ...
  ],
  exports: [
    ListviewComponent,
    AdvancedSearchComponent,
    // ... other exports ...
  ],
  providers: [
    SearchParserService,
    // ... other providers ...
  ]
})
export class AppCommonModule { } 
