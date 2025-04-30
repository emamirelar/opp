import { Component, ViewChild, ViewContainerRef, AfterViewInit } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from './essentials/services/auth.service';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FeedbackDialogComponent } from './common/reusables/widgets/feedback-dialog/feedback-dialog.component';
import { AiAssistantComponent } from './common/reusables/widgets/ai-assistant/ai-assistant.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, 
    RouterModule, 
    ToastModule, 
    ConfirmDialogModule,
    FeedbackDialogComponent, 
    AiAssistantComponent
  ],
  template: `
  <p-confirmDialog></p-confirmDialog>
  <app-feedback-dialog></app-feedback-dialog>
  @if(isLoggedIn) {
    <app-ai-assistant [viewContainerRef]="viewContainerRef"></app-ai-assistant>
  }
    <div #dynamicComponent></div>
  <router-outlet></router-outlet>`,
  standalone: true,
})
export class AppComponent implements AfterViewInit {
  public isExpanded: Boolean = false;
  public isLoggedIn: Boolean = false;
  @ViewChild('dynamicComponent', { read: ViewContainerRef, static: false }) dynamicComponent!: ViewContainerRef;
  viewContainerRef!: ViewContainerRef;
  constructor(
    private authService: AuthService
  ) { }
  ngOnInit() {
    this.authService.isLogedIn().subscribe((res) => {
      this.isLoggedIn = res;
    });
  }
  ngAfterViewInit() {
      this.viewContainerRef = this.dynamicComponent;
  }
}
