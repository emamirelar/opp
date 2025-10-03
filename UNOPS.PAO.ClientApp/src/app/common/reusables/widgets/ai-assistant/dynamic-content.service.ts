import { Injectable, ComponentRef, ViewContainerRef, ComponentFactoryResolver, Injector, ApplicationRef } from '@angular/core';
import { ContentRendererComponent } from './content-renderer/content-renderer.component';
import { ResultItem } from './ai-assistant.model';

// Interface for tracking dynamic components
interface DynamicComponentInfo {
  componentRef: ComponentRef<ContentRendererComponent>;
  type: string;
  renderingId: string;
  completed: boolean;
  lastUpdate: number;
}

// Interface for chunk data processing
interface ChunkData {
  type: string;
  content: any;
  entityType?: string;
  partial: boolean;
  invocationId: string;
  renderingId?: string;
  timestamp: number;
}

@Injectable({
  providedIn: 'root'
})
export class DynamicContentService {
  private viewContainer!: ViewContainerRef;
  private componentFactory: any;
  private activeComponents = new Map<string, DynamicComponentInfo>();
  private cardClickCallback?: (event: any) => void;

  constructor(
    private componentFactoryResolver: ComponentFactoryResolver,
    private injector: Injector,
    private applicationRef: ApplicationRef
  ) {
    // Create component factory for ContentRendererComponent
    this.componentFactory = this.componentFactoryResolver.resolveComponentFactory(ContentRendererComponent);
  }

  setViewContainer(viewContainer: ViewContainerRef): void {
    this.viewContainer = viewContainer;
  }

  setCardClickCallback(callback: (event: any) => void): void {
    this.cardClickCallback = callback;
  }

  processChunk(chunk: any): void {
    if (!this.viewContainer) {
      return;
    }

    // Handle stream completion signal
    if (chunk.streamCompleted) {
      this.markAllComponentsCompleted();
      return;
    }

    // Process each part in the chunk as a separate component
    if (chunk.content?.parts && chunk.content.parts.length > 0) {
      chunk.content.parts.forEach((part: any, partIndex: number) => {
        const chunkData = this.detectChunkTypeFromPart(part, chunk, partIndex);
        if (!chunkData) {
          return;
        }

        const safeRenderingId = chunkData.renderingId || `${chunkData.type}-${Date.now()}`;
        const existingComponent = this.findExistingComponent(chunkData.type, safeRenderingId, chunkData.partial);

        if (existingComponent) {
          this.updateComponent(existingComponent, chunkData.content, !chunkData.partial);
        } else {
          this.createComponent(chunkData, safeRenderingId);
        }
      });
    }
  }

  private detectChunkTypeFromPart(part: any, chunk: any, partIndex: number): ChunkData | null {
    // For partial chunks, use invocationId so they can find and update the same component
    // For complete chunks, use invocationId for final update to same component
    const isPartial = chunk.partial === true;
    let renderingIdBase: string;
    
    
    
    // Use invocationId for chunks from the same conversation stream of the same type
    // This ensures partial chunks update the same component, and final chunk completes it
    // Different content types within the same invocation get separate components
    renderingIdBase = chunk.invocationId;
    
    if (part.thought === true && part.text) {
      return {
        type: 'thought',
        content: part.text,
        partial: isPartial,
        invocationId: chunk.invocationId,
        renderingId: `${renderingIdBase}-thought`,
        timestamp: chunk.timestamp || Date.now()
      };
    }
    
    if (part.text && !part.functionCall && !part.thought) {
      return {
        type: 'markdown',
        content: part.text,
        partial: isPartial,
        invocationId: chunk.invocationId,
        renderingId: `${renderingIdBase}-markdown`,
        timestamp: chunk.timestamp || Date.now()
      };
    }
    
    if (part.functionCall) {
      // Don't render components for functionCall
      return null;
    }
    
    if (part.functionResponse) {
      // Handle function response - especially invoke_app_api
      if (part.functionResponse.name === 'invoke_app_api' && part.functionResponse.response) {
        try {
          // For invoke_app_api, the response is already parsed, not a JSON string
          const parsedResult = part.functionResponse.response;
          
          let cardData = parsedResult;
          
          // The invoke_app_api returns: { status: "success", response: actualData, api_call: "..." }
          // So we need to extract the actual response data
          if (parsedResult.response) {
            cardData = parsedResult.response;
            
            // If the response has records property, use that
            if (parsedResult.response.records) {
              cardData = parsedResult.response.records;
            }
          } else if (parsedResult.records) {
            cardData = parsedResult.records;
          }
          
          // Determine entity type (title case to match switch cases in AI content component)
          let entityType = 'Partner';
          if (parsedResult.api_call?.includes('/api/partner')) {
            entityType = 'Partner';
          } else if (parsedResult.api_call?.includes('/api/contact')) {
            entityType = 'Contact';
          } else if (parsedResult.api_call?.includes('/api/interaction')) {
            entityType = 'Interaction';
          }
          
          return {
            type: 'card',
            content: cardData,
            entityType: entityType, // Pass the detected entity type
            partial: false, // Function responses are always complete
            invocationId: chunk.invocationId,
            renderingId: `${renderingIdBase}-card`,
            timestamp: chunk.timestamp || Date.now()
          };
          } catch (error) {
            // Failed to process function response - don't render anything
          }
      }
      
      // Don't render components for regular functionResponse
      return null;
    }
    
    return null;
  }


  private normalizeChunkType(type: string): string {
    // Map various chunk types to standard types
    const typeMap: { [key: string]: string } = {
      'thoughts': 'thought',
      'markdown': 'markdown',
      'text': 'markdown',
      'card': 'card',
      'grid': 'grid',
      'chart': 'chart',
      'chartjs': 'chartjs',
      'mermaid': 'mermaid',
      'code': 'code',
      'functionCall': 'functionCall',
      'functionResponse': 'functionResponse'
    };
    
    return typeMap[type] || type;
  }

  private findExistingComponent(type: string, renderingId: string, isPartial: boolean): DynamicComponentInfo | null {
    // Look for component with exact matching renderingId that is not completed
    const exactComponent = this.activeComponents.get(renderingId);
    if (exactComponent) {
      if (!exactComponent.completed) {
        return exactComponent;
      }
    }
    
    // If no exact match found or component is completed, return null (will create new component)
    return null;
  }

  private createComponent(chunkData: ChunkData, renderingId: string): void {
    const resultItem: ResultItem = {
      type: this.mapTypeToResultItemType(chunkData.type),
      message: chunkData.content,
      entity: chunkData.entityType, // Pass the entity type to the ResultItem
      partial: chunkData.partial,
      renderingId: renderingId,
      completed: !chunkData.partial,
      timestamp: chunkData.timestamp,
      invocationId: chunkData.invocationId
    };

    // Create component instance
    const componentRef: ComponentRef<ContentRendererComponent> = this.viewContainer.createComponent<ContentRendererComponent>(this.componentFactory, undefined, this.injector);
    
    // Set component inputs using proper Angular input binding mechanism
    componentRef.setInput('item', resultItem);
    componentRef.setInput('shouldShow', true);
    componentRef.setInput('isNewMessage', true);
    componentRef.setInput('renderingId', renderingId);
    componentRef.setInput('isProgressive', chunkData.partial);

    // Wire up cardClicked event if callback is provided
    if (this.cardClickCallback) {
      componentRef.instance.cardClicked.subscribe(this.cardClickCallback);
    }

    // Force Angular change detection to ensure UI updates
    componentRef.changeDetectorRef.detectChanges();
    

    // Store component info
    const componentInfo: DynamicComponentInfo = {
      componentRef: componentRef,
      type: chunkData.type,
      renderingId: renderingId,
      completed: !chunkData.partial,
      lastUpdate: chunkData.timestamp
    };

    this.activeComponents.set(renderingId, componentInfo);
  }

  private updateComponent(componentInfo: DynamicComponentInfo, content: any, markCompleted: boolean): void {
    const { componentRef } = componentInfo;
    
    
    // Handle content update based on type
    let finalContent = content;
    
    if (componentInfo.type === 'thought' || componentInfo.type === 'markdown') {
      // For text-based content, handle concatenation vs replacement
      const currentContent = componentRef.instance.item.message as string || '';
      
      if (markCompleted) {
        // Final update - replace entirely with the complete content
        finalContent = content;
      } else {
        // Partial update - simple concatenation for streaming chunks
        if (typeof content === 'string') {
          // For streaming, each chunk contains new text to append
          finalContent = currentContent + content;
        } else {
          finalContent = content; // Replace if not a string
        }
      }
    } else {
      // For structured content (cards, etc.), always replace
      finalContent = content;
    }
    
    // Update the item content
    const updatedItem: ResultItem = {
      ...componentRef.instance.item,
      message: finalContent,
      completed: markCompleted,
      partial: !markCompleted,
      timestamp: Date.now()
    };

    // CRITICAL: Instead of directly assigning, we need to use Angular's input binding mechanism
    // to ensure ngOnChanges is triggered properly
    
    componentRef.setInput('item', updatedItem);
    componentRef.setInput('isProgressive', !markCompleted);

    // CRITICAL: With OnPush change detection, we need to mark the component for check
    // AND trigger change detection to ensure the UI updates immediately
    componentRef.changeDetectorRef.markForCheck();
    componentRef.changeDetectorRef.detectChanges();
    

    // Update component info
    componentInfo.completed = markCompleted;
    componentInfo.lastUpdate = Date.now();
  }

  private mapTypeToResultItemType(type: string): 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'thoughts' | 'functionCall' | 'functionResponse' | 'chart' {
    const typeMap: { [key: string]: 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'thoughts' | 'functionCall' | 'functionResponse' | 'chart' } = {
      'thought': 'thought',
      'thoughts': 'thoughts',
      'markdown': 'markdown',
      'text': 'text',
      'card': 'card',
      'grid': 'grid',
      'chart': 'chart',
      'chartjs': 'chartjs',
      'mermaid': 'mermaid',
      'code': 'code',
      'functionCall': 'functionCall',
      'functionResponse': 'functionResponse'
    };
    
    return typeMap[type] || 'markdown';
  }

  clearAllComponents(): void {
    // Destroy all component references
    for (const [id, componentInfo] of this.activeComponents) {
      componentInfo.componentRef.destroy();
    }
    
    // Clear the map
    this.activeComponents.clear();
    
    // Clear the view container
    if (this.viewContainer) {
      this.viewContainer.clear();
    }
  }

  getActiveComponentsCount(): number {
    return this.activeComponents.size;
  }

  getActiveComponents(): Map<string, DynamicComponentInfo> {
    return new Map(this.activeComponents);
  }

  markAllComponentsCompleted(): void {
    // Mark all active components as completed
    for (const [id, componentInfo] of this.activeComponents) {
      if (!componentInfo.completed) {
        this.updateComponent(componentInfo, componentInfo.componentRef.instance.item.message, true);
      }
    }
  }
}