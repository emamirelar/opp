import { Component, OnInit } from '@angular/core';
import { TreeTableModule } from 'primeng/treetable';
import { TreeNode } from "primeng/api"; 
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-partner-tree',
  imports: [TreeTableModule, ButtonModule, CommonModule, FormsModule, TableModule],
  templateUrl: './partner-tree.component.html',
  styleUrl: './partner-tree.component.scss'
})
export class PartnerTreeComponent implements OnInit{
  files: TreeNode[] = []; 
    cols:any[] = [];
    constructor() {
    }
    ngOnInit() {
      this.cols = [ 
        { field: "name", header: "Name" }, 
        { field: "description", header: "Short Description" } ,
        { field: "code", header: "Code" },
        { field: "type", header: "Type" },
        { field: "parent", header: "Parent Account Level" },
        { field: "status", header: "Status" },
        //{ field: "action", header: "Action" },
    ]; 
    this.files = [ 
        { 
            data: { 
                name: "Government", 
                description: "Government",
                code: "GOVERNMENT",
                type: "Level 1",
                status: "Active"
            }, 
            children: [ 
                { 
                    data: { 
                      name: "OECD/DAC Government", 
                      description: "Gov: OECD/DAC",
                      code: "OECD_DAC",
                      type: "Level 2",
                      parent: "Government",
                      status: "Active",
                    } ,
                    children: [{
                      data: {
                        name: "Iceland", 
                        description: "Iceland",
                        code: "ICELAND",
                        type: "Level 3",
                        parent: "OECD/DAC Government",
                        status: "Active",
                      }
                    }, {
                      data: {
                        name: "Australia", 
                        description: "Australia",
                        code: "AUSTRALIA",
                        type: "Level 3",
                        parent: "OECD/DAC Government",
                        status: "Active",
                      }
                    }, {
                      data: {
                        name: "Greece", 
                        description: "Greece",
                        code: "GREECE",
                        type: "Level 3",
                        parent: "OECD/DAC Government",
                        status: "Active",
                      }
                    },{
                      data: {
                        name: "Norway", 
                        description: "Norway",
                        code: "NORWAY",
                        type: "Level 3",
                        parent: "OECD/DAC Government",
                        status: "Active",
                      }
                    }]
                }, 
                { 
                    data: { 
                      name: "Non-OECD/DAC Government", 
                      description: "Gov: Non-OECD/DAC",
                      code: "NON_OECD_DAC",
                      type: "Level 2",
                      parent: "Government",
                      status: "Active"
                    },
                    children: [{
                      data: {
                        name: "Mexico", 
                        description: "Mexico",
                        code: "MEXICO",
                        type: "Level 3",
                        parent: "Non-OECD/DAC Government",
                        status: "Active",
                      }
                    }, {
                      data: {
                        name: "Panama", 
                        description: "Panama",
                        code: "PANAMA",
                        type: "Level 3",
                        parent: "Non-OECD/DAC Government",
                        status: "Active",
                      }
                    }, {
                      data: {
                        name: "Qatar", 
                        description: "Qatar",
                        code: "QATAR",
                        type: "Level 3",
                        parent: "Non-OECD/DAC Government",
                        status: "Active",
                      }
                    }, {
                      data: {
                        name: "India", 
                        description: "India",
                        code: "INDIA",
                        type: "Level 3",
                        parent: "Non-OECD/DAC Government",
                        status: "Active",
                      }
                    }]
                } 
            ] 
        }, 
            ]; 
    }
    

    handleOnSaveClick() {
  
      
    }
}
