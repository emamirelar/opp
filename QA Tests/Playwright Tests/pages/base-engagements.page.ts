/**
 * @fileoverview Base Engagements Page Object
 * Page object for the UNOPS base engagement list page.
 * Route: /internal/base-engagements
 */

import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class BaseEngagementsPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  get list(): Locator {
    return this.page.getByText(/showing\s+\d+\s+record/i).first()
      .or(this.page.getByPlaceholder(/search/i).first())
      .or(this.page.locator('p-table, .p-datatable, app-listview').first())
      .or(this.page.locator('table').first())
      .or(this.page.locator('[data-testid="base-engagements-listview"]').first());
  }

  get emptyState(): Locator {
    return this.page.getByText(/no .*(engagement|record|data)/i).first()
      .or(this.page.getByText(/coming soon/i).first())
      .or(this.page.locator('.p-datatable-emptymessage').first())
      .or(this.page.locator('.empty-state').first())
      .or(this.page.locator('[data-testid="empty-state"]').first());
  }

  get searchInput(): Locator {
    return this.page.getByPlaceholder(/search/i).first()
      .or(this.page.locator('input[type="text"], input[type="search"]').first());
  }

  get addButton(): Locator {
    return this.page.getByRole('button', { name: /new|add|create/i }).first()
      .or(this.page.locator('button').filter({ hasText: /add|new|create/i }).first());
  }

  get addIcon(): Locator {
    return this.page.locator('button .pi-plus')
      .or(this.page.locator('button [class*="plus"]'))
      .first();
  }

  get paginator(): Locator {
    return this.page.locator('p-paginator')
      .or(this.page.locator('.p-paginator'))
      .first();
  }

  get loadMoreButton(): Locator {
    return this.page.getByRole('button', { name: /load more|show more/i })
      .or(this.page.locator('button').filter({ hasText: /load more|show more/i }))
      .first();
  }

  get firstListItem(): Locator {
    return this.page.locator('tbody tr, .p-datatable-tbody tr, app-listview-card .cursor-pointer').first();
  }

  async navigate(): Promise<void> {
    await this.goto('/internal/base-engagements');
  }
}
