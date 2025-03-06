import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table'; // Ensure this import
import { TreeTableModule } from 'primeng/treetable'; // Ensure this import
import { ButtonModule } from 'primeng/button'; // Ensure this import
import { ToggleButtonModule } from 'primeng/togglebutton'; // Ensure this import
// ...existing imports...

@NgModule({
  declarations: [
    // ...existing declarations...
  ],
  imports: [
    CommonModule,
    FormsModule,
    TableModule, // Ensure this import
    TreeTableModule, // Ensure this import
    ButtonModule, // Ensure this import
    ToggleButtonModule, // Ensure this import
    // ...existing imports...
  ],
  providers: [
    // ...existing providers...
  ],
})
export class InternalModule { }
