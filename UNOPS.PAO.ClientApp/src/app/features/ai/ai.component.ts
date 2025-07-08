import { Component, ViewChild, ViewContainerRef, inject, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AiAssistantPanelComponent } from '../../common/reusables/widgets/ai-assistant/ai-assistant-panel.component';

@Component({
  selector: 'app-ai',
  standalone: true,
  imports: [CommonModule, AiAssistantPanelComponent],
  template: `
    <div class="h-full flex flex-col">
      <app-ai-assistant-panel
        class="h-full w-full"
        [viewContainerRef]="viewContainerRef"
        [hideHeader]="true">
      </app-ai-assistant-panel>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      width: 100%;
    }
  `]
})
export class AiComponent implements AfterViewInit {
  @ViewChild('dynamicComponent', { read: ViewContainerRef, static: false }) dynamicComponent!: ViewContainerRef;
  viewContainerRef!: ViewContainerRef;

  ngAfterViewInit() {
    this.viewContainerRef = this.dynamicComponent;
  }
} 