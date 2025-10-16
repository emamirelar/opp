import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

// Re-export common Angular modules
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

/**
 * SharedModule - Contains truly reusable components, directives, and pipes
 * 
 * This module exports components that are used across 3+ feature modules.
 * Components specific to a single feature should stay in that feature module.
 * 
 * Organization:
 * - components/     : Reusable UI components organized by category
 * - directives/     : Shared directives (e.g., permissions, validation)
 * - pipes/          : Shared pipes (e.g., formatting, transformation)
 * - services/       : Shared services categorized by purpose (api, ui, utils, domain, user, integration)
 * - models/         : Shared data models and interfaces
 * - base-classes/   : Abstract base classes for inheritance
 */
@NgModule({
  declarations: [
    // Import and declare shared components, directives, and pipes here as needed
    // Example:
    // PhoneInputComponent,
    // EntityTagsComponent,
    // HasPermissionDirective,
    // MarkdownPipe,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  exports: [
    // Re-export common modules for convenience
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    
    // Export shared components, directives, and pipes
    // Example:
    // PhoneInputComponent,
    // EntityTagsComponent,
    // HasPermissionDirective,
    // MarkdownPipe,
  ]
})
export class SharedModule { }


