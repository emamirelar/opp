import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { OrganizationHierarchyTreeModel, PrimeOrgChartNode } from '../../models/organization-hierarchy.model';
import { TreeNode } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class OrganizationHierarchyService {
  private apiUrl = '/api/organization-hierarchy';

  constructor(private http: HttpClient) { }

  // Get organization hierarchy data optimized for PrimeNG
  getOrganizationHierarchy(): Observable<TreeNode[]> {
    
    
    return this.http.get<PrimeOrgChartNode[]>(this.apiUrl).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error fetching organization hierarchy:', error);
        
        
        // If the optimized endpoint fails, try the legacy endpoint
        return this.http.get<any[]>(`${this.apiUrl}/legacy`).pipe(
          catchError((legacyError: HttpErrorResponse) => {
            console.error('Error fetching from legacy endpoint:', legacyError);
            
            
            // If both API calls fail, return test data as fallback
            return of(this.getTestData());
          }),
          map(legacyResponse => {
            
            return this.transformToPrimeNgFormat(legacyResponse);
          })
        );
      }),
      map(response => {
        
        
        // If the API already returns data in the correct format, return it directly
        if (response && response.length > 0 && 'expanded' in response[0]) {
          
          return response as TreeNode[];
        }
        
        // Otherwise, transform the data to match the required format
        
        return this.transformToPrimeNgFormat(response);
      })
    );
  }
  
  // Get test data for fallback
  private getTestData(): TreeNode[] {
    
    return [
      {
        expanded: true,
        type: 'person',
        data: {
          id: 1,
          name: 'United Nations Office for Project Services',
          code: 'OPS',
          type: 0,
          description: 'Main organization unit'
        },
        children: [
          {
            expanded: true,
            type: 'person',
            data: {
              id: 2,
              name: 'Business Group 1',
              code: 'BG1',
              type: 1,
              description: 'First business group'
            },
            children: [
              {
                expanded: false,
                type: 'person',
                data: {
                  id: 4,
                  name: 'Country Office 1',
                  code: 'CO1',
                  type: 2,
                  description: 'Country office in region 1'
                },
                children: []
              }
            ]
          }
        ]
      }
    ];
  }
  
  // Transform legacy API response format to PrimeNG TreeNode format
  private transformToPrimeNgFormat(data: any[]): TreeNode[] {
    if (!data || data.length === 0) {
      console.warn('No data to transform');
      return [];
    }
    
    const result: TreeNode[] = [];
    
    // Process each top-level node
    data.forEach(item => {
      if (!item || !item.data) {
        console.warn('Invalid item in API response:', item);
        return;
      }
      
      const nodeData = item.data;
      const node: TreeNode = {
        expanded: true,
        type: 'person',
        data: {
          id: nodeData.id,
          name: nodeData.name || 'Unnamed',
          code: nodeData.code || 'No Code',
          type: typeof nodeData.type === 'number' ? nodeData.type : 0,
          description: nodeData.description || 'No description provided',
          parentId: nodeData.parentId
        },
        children: []
      };
      
      // Process children recursively if they exist
      if (nodeData.children && nodeData.children.length > 0) {
        node.children = this.processChildren(nodeData.children);
      }
      
      result.push(node);
    });
    
    
    return result;
  }
  
  // Process children nodes recursively
  private processChildren(children: any[]): TreeNode[] {
    return children.map(child => {
      const node: TreeNode = {
        expanded: false,
        type: 'person',
        data: {
          id: child.id,
          name: child.name || 'Unnamed',
          code: child.code || 'No Code',
          type: typeof child.type === 'number' ? child.type : 0,
          description: child.description || 'No description provided',
          parentId: child.parentId
        },
        children: []
      };
      
      if (child.children && child.children.length > 0) {
        node.children = this.processChildren(child.children);
      }
      
      return node;
    });
  }
} 
