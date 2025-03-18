import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from './essentials/services/auth.service';
import { ToastModule } from 'primeng/toast';
import { FeedbackDialogComponent } from './common/reusables/widgets/feedback-dialog/feedback-dialog.component';
import { AiAssistantComponent } from './common/reusables/widgets/ai-assistant/ai-assistant.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, ToastModule, FeedbackDialogComponent, AiAssistantComponent],
  template: `
  <app-feedback-dialog></app-feedback-dialog>
  @if(isLoggedIn) {
    <app-ai-assistant></app-ai-assistant>
  }
  <router-outlet></router-outlet>`,
  standalone: true,
})
export class AppComponent {
  public isExpanded: Boolean = false;
  public isLoggedIn: Boolean = false;
  constructor(
    private authService: AuthService,
  ) { }
  ngOnInit() {
    this.authService.isLogedIn().subscribe((res) => {
      this.isLoggedIn = res;
    });
  }
}
