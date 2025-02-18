import { Component, OnInit, ElementRef, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, signal } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuComponent } from '../menu/menu.component';
import { AuthService } from '../../../../essentials/services/auth.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

@Component({
  selector: 'app-sidebar',
  imports: [MenuComponent],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent implements OnInit, OnDestroy {
  constructor(
    public el: ElementRef,
    private authService: AuthService,
    public languageService: LanguageService,
    private cdr: ChangeDetectorRef
  ) { }

  private langChangeSubscription: Subscription = new Subscription;

  menuItems: MenuItem[] = [
    {
      label: 'title.home',
      icon: 'home',
      routerLink: ['/'],
    },
    {
      label: 'title.fundingOpportunities',
      icon: 'money_bag',
      routerLink: ['/funding-opportunity'],
    },
    {
      label: 'title.proposals',
      icon: 'receipt',
      routerLink: ['/proposal'],
    },
  ];

  externalMenuItems: MenuItem[] = [
    {
      label: 'title.home',
      icon: 'home',
      routerLink: ['/'],
    },
    {
      label: 'title.fundingOpportunities',
      icon: 'money_bag',
      routerLink: ['/external/funding-opportunity'],
    },
    {
      label: 'title.external.myProposals',
      icon: 'receipt',
      routerLink: ['/external/proposal'],
    },
  ];

  isInternalUser = signal<boolean>(false);

  ngOnInit() {
    this.authService.isInternal().subscribe((isInternal) => {
      this.isInternalUser.set(isInternal);
    });

    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }
}
