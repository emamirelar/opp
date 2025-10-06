import { Injectable, ComponentRef, ViewContainerRef, ComponentFactoryResolver, Injector, ApplicationRef } from '@angular/core';
import { ContentRendererComponent } from './content-renderer/content-renderer.component';
import { ContentPart } from './ai-assistant.model';

// Interface for tracking dynamic components
interface DynamicComponentInfo {
  componentRef: ComponentRef<ContentRendererComponent>;
  type: string;
  renderingId: string;
  completed: boolean;
  lastUpdate: number;
}

// Use ContentPart from ai-assistant.model.ts - no need for separate ChunkData interface

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
      console.warn('⚠️ No view container set, cannot process chunk');
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
        const contentPart = this.detectChunkTypeFromPart(part, chunk, partIndex);
        if (!contentPart) {
          return;
        }

        // Add role information to content part - check multiple sources for user role
        contentPart.isUserMessage = chunk.role === 'user' || chunk.isUser === true || chunk.author === 'user';

        const safeRenderingId = contentPart.renderingId || `${chunk.invocationId}-${contentPart.type}`;
        const existingComponent = this.findExistingComponent(contentPart.type!, safeRenderingId, contentPart.partial || false);

        if (existingComponent) {
          // Update existing component following documented logic
          // Mark as completed if partial is false or undefined
          const markCompleted = !contentPart.partial; // true if partial is false or undefined
          this.updateComponent(existingComponent, contentPart.content, markCompleted);
        } else {
          // Create new component following documented logic
          this.createComponent(contentPart, safeRenderingId);
        }
      });
    }
  }

  private detectChunkTypeFromPart(part: any, chunk: any, partIndex: number): ContentPart | null {
    // Use the documented logic: invocationId-based renderingId for both streaming and session loading
    // All parts in a chunk inherit the chunk's partial flag
    const isPartial = chunk.partial === true;
    let renderingIdBase: string;
    
    // Use invocationId for chunks from the same conversation stream of the same type
    // This ensures partial chunks update the same component, and final chunk completes it
    // Different content types within the same invocation get separate components
    renderingIdBase = chunk.invocationId;
    
    if (part.thought === true && part.text) {
      return {
        text: part.text,
        thought: true,
        type: 'thought',
        content: part.text,
        partial: isPartial, // All parts inherit chunk's partial flag
        invocationId: chunk.invocationId,
        renderingId: `${renderingIdBase}-thought`,
        timestamp: chunk.timestamp || Date.now()
      };
    }
    
    if (part.text && !part.functionCall && !part.thought) {
      // Check if this is a user message - use special user-message type
      const isUserMessage = chunk.role === 'user' || chunk.isUser === true || chunk.author === 'user';
      const contentType = isUserMessage ? 'user-message' : 'markdown';
      
      return {
        text: part.text,
        type: contentType,
        content: part.text,
        partial: isUserMessage ? false : isPartial, // User messages are always complete, AI messages use actual partial flag
        invocationId: chunk.invocationId,
        renderingId: `${renderingIdBase}-${contentType}`,
        timestamp: chunk.timestamp || Date.now(),
        isUserMessage: isUserMessage
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
            entity: cardData, // Set entity to the card data for content-renderer to access
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
    // Look for component with exact matching renderingId
    const exactComponent = this.activeComponents.get(renderingId);
    if (exactComponent) {
      // CRITICAL: Only return existing component if it's not completed
      // Completed components should not be updated - they are frozen
      if (exactComponent.completed) {
        return null; // Will create new component instead
      }
      
      // For partial chunks, return existing component to allow updates
      // For final chunks, return existing component to allow final updates
      return exactComponent;
    }
    
    // If no exact match found, return null (will create new component)
    return null;
  }

  private createComponent(contentPart: ContentPart, renderingId: string): void {
    // Use the contentPart directly - it's already properly structured

    // Create component instance - explicitly append to end of view container
    // Use undefined index to append to the end, ensuring chronological order
    const componentRef: ComponentRef<ContentRendererComponent> = this.viewContainer.createComponent<ContentRendererComponent>(this.componentFactory, undefined, this.injector);
    
    // Set component inputs using proper Angular input binding mechanism
    componentRef.setInput('item', contentPart);
    componentRef.setInput('shouldShow', true);
    componentRef.setInput('isNewMessage', true);
    componentRef.setInput('renderingId', renderingId);
    componentRef.setInput('isProgressive', contentPart.partial || false);
    componentRef.setInput('isUserMessage', contentPart.isUserMessage || false);

    // Wire up cardClicked event if callback is provided
    if (this.cardClickCallback) {
      componentRef.instance.cardClicked.subscribe(this.cardClickCallback);
    }

    // Apply appropriate styling based on message role
    this.applyMessageStyling(componentRef, contentPart.isUserMessage || false);

    // Force Angular change detection to ensure UI updates
    componentRef.changeDetectorRef.detectChanges();
    
    // Store component info following documented logic
    // Mark as completed if partial is false or undefined
    const isCompleted = !contentPart.partial; // true if partial is false or undefined
    
    const componentInfo: DynamicComponentInfo = {
      componentRef: componentRef,
      type: contentPart.type!,
      renderingId: renderingId,
      completed: isCompleted, // Mark as completed if partial is false or undefined
      lastUpdate: contentPart.timestamp || Date.now()
    };

    this.activeComponents.set(renderingId, componentInfo);
  }

  private updateComponent(componentInfo: DynamicComponentInfo, content: any, markCompleted: boolean): void {
    const { componentRef } = componentInfo;
    
    // Handle content update based on type
    let finalContent = content;
    
    if (componentInfo.type === 'thought' || componentInfo.type === 'markdown') {
      // For text-based content, handle concatenation vs replacement
      const currentContent = componentRef.instance.item.text || '';
      
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
    const updatedItem: ContentPart = {
      ...componentRef.instance.item,
      content: finalContent,
      entity: finalContent,
      completed: markCompleted,
      text: (componentInfo.type === 'thought' || componentInfo.type === 'markdown' || componentInfo.type === 'user-message' || componentInfo.type === 'text') ? finalContent : componentRef.instance.item.text
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

  private mapTypeToContentPartType(type: string): 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'thoughts' | 'functionCall' | 'functionResponse' | 'chart' | 'user-message' {
    const typeMap: { [key: string]: 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'thoughts' | 'functionCall' | 'functionResponse' | 'chart' | 'user-message' } = {
      'thought': 'thought',
      'thoughts': 'thoughts',
      'markdown': 'markdown',
      'user-message': 'user-message',
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
      try {
        componentInfo.componentRef.destroy();
      } catch (error) {
        console.warn('Error destroying component:', error);
      }
    }
    
    // Clear the map
    this.activeComponents.clear();
    
    // Clear the view container
    if (this.viewContainer) {
      try {
        this.viewContainer.clear();
      } catch (error) {
        console.warn('Error clearing view container:', error);
      }
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
        this.updateComponent(componentInfo, componentInfo.componentRef.instance.item.text || componentInfo.componentRef.instance.item.entity, true);
      }
    }
  }

  private applyMessageStyling(componentRef: ComponentRef<ContentRendererComponent>, isUserMessage: boolean): void {
    const element = componentRef.location.nativeElement;
    
    // Remove all inline styles - let CSS handle the styling
    element.style.cssText = '';
    
    // Only apply essential classes for styling
    if (isUserMessage) {
      element.classList.add('user-message');
    } else {
      element.classList.add('ai-message');
    }
  }
}