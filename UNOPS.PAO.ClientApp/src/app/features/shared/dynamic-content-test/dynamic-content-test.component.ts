import { Component, ViewChild, ViewContainerRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextarea } from 'primeng/inputtextarea';
import { CardModule } from 'primeng/card';
import { DynamicContentService } from '@shared/reusables/widgets/ai-assistant/dynamic-content.service';

@Component({
  selector: 'app-dynamic-content-test',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextarea, CardModule],
  template: `
    <div class="p-4">
      <p-card header="Dynamic Content Service Tester">
        <div class="grid">
          <div class="col-6">
            <h3>Input</h3>
            <textarea 
              pInputTextarea 
              [(ngModel)]="chunkJson" 
              rows="15" 
              class="w-full"
              placeholder="Paste chunk JSON here...">
            </textarea>
            <div class="mt-3 flex gap-2">
              <p-button 
                label="Process Chunk" 
                (onClick)="processChunk()"
                [disabled]="!chunkJson.trim()">
              </p-button>
              <p-button 
                label="Clear All" 
                (onClick)="clearAll()"
                severity="secondary">
              </p-button>
              <p-button 
                label="Load Sample 1" 
                (onClick)="loadSample1()"
                severity="help">
              </p-button>
              <p-button 
                label="Load Sample 2" 
                (onClick)="loadSample2()"
                severity="help">
              </p-button>
              <p-button 
                label="Load Sample 3" 
                (onClick)="loadSample3()"
                severity="help">
              </p-button>
            </div>
            
            <div class="mt-4">
              <h4>Component Status</h4>
              <div class="text-sm">
                <div>Active Components: {{ getActiveComponentCount() }}</div>
                <div *ngFor="let comp of getActiveComponentsInfo()" class="mt-1 p-2 border-1 surface-border border-round">
                  <div><strong>ID:</strong> {{ comp.renderingId }}</div>
                  <div><strong>Type:</strong> {{ comp.type }}</div>
                  <div><strong>Completed:</strong> {{ comp.completed }}</div>
                  <div><strong>Content Length:</strong> {{ comp.contentLength }}</div>
                </div>
              </div>
            </div>
          </div>
          
          <div class="col-6">
            <h3>Rendered Output</h3>
            <div class="border-1 surface-border border-round p-3 min-h-20rem">
              <!-- Dynamic content container -->
              <div #dynamicContentContainer class="dynamic-content-container"></div>
            </div>
          </div>
        </div>
      </p-card>
    </div>
  `,
  styles: [`
    .dynamic-content-container {
      min-height: 200px;
    }
  `]
})
export class DynamicContentTestComponent {
  @ViewChild('dynamicContentContainer', { read: ViewContainerRef }) 
  private dynamicContentContainer!: ViewContainerRef;

  chunkJson = '';
  
  private dynamicContentService = inject(DynamicContentService);

  ngAfterViewInit() {
    // Set the view container for the dynamic content service
    this.dynamicContentService.setViewContainer(this.dynamicContentContainer);
  }

  processChunk() {
    try {
      const chunk = JSON.parse(this.chunkJson);
      console.log('🧪 TEST: Processing chunk:', chunk);
      this.dynamicContentService.processChunk(chunk);
    } catch (error) {
      console.error('❌ TEST: Invalid JSON:', error);
      alert('Invalid JSON format');
    }
  }

  clearAll() {
    this.dynamicContentService.clearAllComponents();
    this.chunkJson = '';
    console.log('🧪 TEST: Cleared all components');
  }

  getActiveComponentCount(): number {
    return this.dynamicContentService.getActiveComponentsCount();
  }

  getActiveComponentsInfo(): any[] {
    const components = this.dynamicContentService.getActiveComponents();
    return Array.from(components.entries()).map(([id, comp]) => ({
      renderingId: comp.renderingId,
      type: comp.type,
      completed: comp.completed,
      contentLength: typeof comp.componentRef.instance.item.message === 'string' 
        ? comp.componentRef.instance.item.message.length 
        : JSON.stringify(comp.componentRef.instance.item.message).length
    }));
  }

  loadSample1() {
    this.chunkJson = JSON.stringify({
      "content": {
        "parts": [
          {
            "thought": true,
            "text": "**Defining the Policy's Scope**\n\nI'm focusing on the phrase \"engagement acceptance policy\" now. My initial thought is that this refers to a formal corporate document. I'm preparing to use my search tool on a relevant corporate vector store.\n\n\n"
          }
        ],
        "role": "model"
      },
      "partial": true,
      "invocationId": "e-51e57624-1684-40a2-8206-c36dc4acfcf4",
      "id": "be45018a-f4d1-4605-9a99-cd8031d50433",
      "timestamp": 1759226811.770251
    }, null, 2);
  }

  loadSample2() {
    this.chunkJson = JSON.stringify({
      "content": {
        "parts": [
          {
            "thought": true,
            "text": "**Searching the Corporate Store**\n\nI've decided to use the `search_corp_vector_store` tool. I'll search with \"engagement acceptance policy\" as the query. To refine the results, I plan to include the `entityTypeId` parameter, setting it to \"POLICY\" to focus the results. This should quickly pinpoint the precise document the user is requesting.\n\n\n"
          }
        ],
        "role": "model"
      },
      "partial": true,
      "invocationId": "e-51e57624-1684-40a2-8206-c36dc4acfcf4",
      "id": "cf820640-4676-4f20-9dd2-569c835c8169",
      "timestamp": 1759226817.633772
    }, null, 2);
  }

  loadSample3() {
    this.chunkJson = JSON.stringify({
      "content": {
        "parts": [
          {
            "thought": true,
            "text": "**Defining the Policy's Scope**\n\nI'm focusing on the phrase \"engagement acceptance policy\" now. My initial thought is that this refers to a formal corporate document. I'm preparing to use my search tool on a relevant corporate vector store.\n\n\n**Searching the Corporate Store**\n\nI've decided to use the `search_corp_vector_store` tool. I'll search with \"engagement acceptance policy\" as the query. To refine the results, I plan to include the `entityTypeId` parameter, setting it to \"POLICY\" to focus the results. This should quickly pinpoint the precise document the user is requesting.\n\n\n"
          }
        ],
        "role": "model"
      },
      "invocationId": "e-51e57624-1684-40a2-8206-c36dc4acfcf4",
      "id": "9903ade1-44cb-425e-9440-234cf55f0c21",
      "timestamp": 1759226818.238033
    }, null, 2);
  }
}
