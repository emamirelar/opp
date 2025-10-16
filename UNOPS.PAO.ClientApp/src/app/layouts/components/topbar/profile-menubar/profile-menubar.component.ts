import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';
import { MenuItem } from 'primeng/api';
import { AuthService } from '@core/services/auth';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '@shared/services/utils';
import { Subscription } from 'rxjs';
import {Ripple} from 'primeng/ripple';

@Component({
  selector: 'app-profile-menubar',
  imports: [MenubarModule, MenuModule, ButtonModule, TranslateModule, Ripple],
  templateUrl: './profile-menubar.component.html',
  styleUrl: './profile-menubar.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileMenubarComponent implements OnInit, OnDestroy {
  menuItems: MenuItem[] = [];

  profileItems: MenuItem[] = [];
  private langChangeSubscription: Subscription = new Subscription;

  constructor(private authService: AuthService, private router: Router, private languageService: LanguageService, private cdr: ChangeDetectorRef)
  {
    this.profileItems = [];
  }

  ngOnInit(): void {
    this.languageService.translateMenuItems(this.profileItems);
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }
}
