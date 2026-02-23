import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <mat-toolbar class="app-toolbar">
      <div class="toolbar-brand" routerLink="/people">
        <mat-icon>rocket_launch</mat-icon>
        <span class="brand-text">Stargate ACTS</span>
      </div>
      <nav class="toolbar-nav">
        <a routerLink="/people" routerLinkActive="active" mat-button>
          <mat-icon>people</mat-icon>
          Personnel
        </a>
      </nav>
    </mat-toolbar>
    <main class="app-content">
      <router-outlet />
    </main>
  `,
    styles: [`
    :host {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .app-toolbar {
      background: var(--color-bg-secondary) !important;
      border-bottom: 1px solid var(--color-border);
      padding: 0 var(--space-xl);
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .toolbar-brand {
      display: flex;
      align-items: center;
      gap: var(--space-sm);
      cursor: pointer;
      color: var(--color-text-primary);
      font-weight: 700;
      font-size: var(--font-size-lg);

      mat-icon {
        color: var(--color-accent-light);
        font-size: 28px;
        width: 28px;
        height: 28px;
      }

      .brand-text {
        background: linear-gradient(135deg, var(--color-accent-light), var(--color-info));
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        background-clip: text;
      }
    }

    .toolbar-nav {
      margin-left: auto;

      a {
        color: var(--color-text-secondary) !important;
        font-weight: 500;

        &.active {
          color: var(--color-accent-light) !important;
        }
      }
    }

    .app-content {
      flex: 1;
      padding: var(--space-xl);
    }
  `]
})
export class AppComponent { }
