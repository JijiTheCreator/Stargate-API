import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { PersonService } from '../../services/person.service';
import { PersonAstronaut } from '../../models/api.models';

@Component({
    selector: 'app-person-detail',
    standalone: true,
    imports: [MatCardModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatChipsModule, RouterLink],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <div class="page-container">
      <a routerLink="/people" class="back-link">
        <mat-icon>arrow_back</mat-icon> Back to Personnel
      </a>

      @if (loading()) {
        <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
      }

      @if (error()) {
        <mat-card class="error-card">
          <mat-card-content>
            <mat-icon>error_outline</mat-icon>
            <p>{{ error() }}</p>
            <button mat-stroked-button (click)="loadPerson()">
              <mat-icon>refresh</mat-icon> Retry
            </button>
          </mat-card-content>
        </mat-card>
      }

      @if (person(); as p) {
        <div class="detail-grid">
          <mat-card class="profile-card">
            <div class="profile-header">
              <div class="avatar-large">{{ p.name.charAt(0) }}</div>
              <div>
                <h1>{{ p.name }}</h1>
                <span class="status-badge" [class]="getStatusClass(p)">
                  {{ p.currentDutyTitle || 'Civilian' }}
                </span>
              </div>
            </div>

            <div class="profile-stats">
              @if (p.currentRank) {
                <div class="stat">
                  <span class="stat-label">Current Rank</span>
                  <span class="stat-value">{{ p.currentRank }}</span>
                </div>
              }
              @if (p.careerStartDate) {
                <div class="stat">
                  <span class="stat-label">Career Start</span>
                  <span class="stat-value">{{ formatDate(p.careerStartDate) }}</span>
                </div>
              }
              @if (p.careerEndDate) {
                <div class="stat">
                  <span class="stat-label">Career End</span>
                  <span class="stat-value">{{ formatDate(p.careerEndDate) }}</span>
                </div>
              }
            </div>

            <div class="profile-actions">
              <a [routerLink]="['/duties', p.name]" mat-fab extended color="primary">
                <mat-icon>assignment</mat-icon>
                View Duty History
              </a>
            </div>
          </mat-card>
        </div>
      }
    </div>
  `,
    styles: [`
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: var(--space-xs);
      margin-bottom: var(--space-lg);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      transition: color 0.2s;

      &:hover { color: var(--color-accent-light); }
    }

    .detail-grid {
      max-width: 600px;
    }

    .profile-card {
      padding: var(--space-xl);
    }

    .profile-header {
      display: flex;
      align-items: center;
      gap: var(--space-lg);
      margin-bottom: var(--space-xl);

      h1 {
        font-size: var(--font-size-2xl);
        font-weight: 700;
        color: var(--color-text-primary);
        margin-bottom: var(--space-xs);
      }
    }

    .avatar-large {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--color-accent), var(--color-info));
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: var(--font-size-2xl);
      font-weight: 700;
      color: white;
      flex-shrink: 0;
    }

    .profile-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: var(--space-lg);
      padding: var(--space-lg);
      background: var(--color-bg-secondary);
      border-radius: var(--radius-md);
      margin-bottom: var(--space-xl);

      .stat {
        display: flex;
        flex-direction: column;
        gap: var(--space-xs);
      }

      .stat-label {
        font-size: var(--font-size-xs);
        color: var(--color-text-muted);
        text-transform: uppercase;
        letter-spacing: 0.05em;
        font-weight: 600;
      }

      .stat-value {
        font-size: var(--font-size-lg);
        font-weight: 600;
        color: var(--color-text-primary);
      }
    }

    .profile-actions {
      display: flex;
      gap: var(--space-md);
    }

    .error-card {
      text-align: center;
      padding: var(--space-2xl);

      mat-card-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-md);

        mat-icon {
          font-size: 48px; width: 48px; height: 48px;
          color: var(--color-error);
        }
        p { color: var(--color-text-secondary); }
      }
    }
  `]
})
export class PersonDetailComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly personService = inject(PersonService);

    readonly person = signal<PersonAstronaut | null>(null);
    readonly loading = signal(true);
    readonly error = signal<string | null>(null);

    ngOnInit(): void {
        this.loadPerson();
    }

    loadPerson(): void {
        const name = this.route.snapshot.paramMap.get('name') ?? '';
        this.loading.set(true);
        this.error.set(null);
        this.personService.getPersonByName(name).subscribe({
            next: (res) => {
                this.person.set(res.person);
                this.loading.set(false);
            },
            error: () => {
                this.error.set('Failed to load person details.');
                this.loading.set(false);
            }
        });
    }

    getStatusClass(person: PersonAstronaut): string {
        if (!person.currentDutyTitle) return 'unknown';
        if (person.currentDutyTitle === 'RETIRED') return 'retired';
        return 'active';
    }

    formatDate(dateStr: string | null): string {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    }
}
