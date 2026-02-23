import { Component, ChangeDetectionStrategy, signal, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AstronautDutyService } from '../../services/astronaut-duty.service';
import { PersonAstronaut, AstronautDuty } from '../../models/api.models';
import { AddDutyFormComponent } from '../add-duty-form/add-duty-form.component';

@Component({
    selector: 'app-duty-history',
    standalone: true,
    imports: [
        MatCardModule, MatButtonModule, MatIconModule, MatProgressBarModule,
        MatDialogModule, MatSnackBarModule, RouterLink
    ],
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
            <button mat-stroked-button (click)="loadDuties()">
              <mat-icon>refresh</mat-icon> Retry
            </button>
          </mat-card-content>
        </mat-card>
      }

      @if (!loading() && !error()) {
        <div class="page-header">
          <div class="header-row">
            <div>
              <h1>{{ personName }} — Duty History</h1>
              @if (person(); as p) {
                <p>{{ p.currentRank }} · {{ p.currentDutyTitle || 'No active duty' }}</p>
              }
            </div>
            <button mat-fab extended color="primary" (click)="openAddDuty()" aria-label="Assign new duty">
              <mat-icon>add_task</mat-icon>
              Assign Duty
            </button>
          </div>
        </div>

        @if (duties().length === 0) {
          <mat-card class="empty-card">
            <mat-card-content>
              <mat-icon>assignment_late</mat-icon>
              <p>No duty assignments found</p>
              <button mat-flat-button color="primary" (click)="openAddDuty()">
                Assign First Duty
              </button>
            </mat-card-content>
          </mat-card>
        } @else {
          <div class="timeline">
            @for (duty of duties(); track duty.id; let i = $index) {
              <div class="timeline-item" [class.current]="!duty.dutyEndDate" [class.retired]="duty.dutyTitle === 'RETIRED'">
                <div class="timeline-marker">
                  <div class="marker-dot"></div>
                  @if (i < duties().length - 1) {
                    <div class="marker-line"></div>
                  }
                </div>
                <mat-card class="duty-card">
                  <div class="duty-header">
                    <div class="duty-title">
                      <h3>{{ duty.dutyTitle }}</h3>
                      @if (!duty.dutyEndDate) {
                        <span class="status-badge active">Current</span>
                      }
                      @if (duty.dutyTitle === 'RETIRED') {
                        <span class="status-badge retired">Retired</span>
                      }
                    </div>
                    <span class="duty-rank">{{ duty.rank }}</span>
                  </div>
                  <div class="duty-dates">
                    <mat-icon>date_range</mat-icon>
                    <span>{{ formatDate(duty.dutyStartDate) }}
                      @if (duty.dutyEndDate) {
                        — {{ formatDate(duty.dutyEndDate) }}
                      } @else {
                        — Present
                      }
                    </span>
                  </div>
                </mat-card>
              </div>
            }
          </div>
        }
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
      &:hover { color: var(--color-accent-light); }
    }

    .header-row {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-md);
    }

    .timeline {
      max-width: 700px;
    }

    .timeline-item {
      display: flex;
      gap: var(--space-lg);
    }

    .timeline-marker {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding-top: var(--space-lg);

      .marker-dot {
        width: 14px;
        height: 14px;
        border-radius: 50%;
        background: var(--color-border);
        border: 3px solid var(--color-bg-primary);
        flex-shrink: 0;
        z-index: 1;
      }

      .marker-line {
        width: 2px;
        flex: 1;
        background: var(--color-border);
      }
    }

    .timeline-item.current .marker-dot {
      background: var(--color-accent-light);
      box-shadow: 0 0 8px var(--color-accent-glow);
    }

    .timeline-item.retired .marker-dot {
      background: var(--color-error);
    }

    .duty-card {
      flex: 1;
      margin-bottom: var(--space-lg);
      padding: var(--space-lg);
      transition: transform 0.2s ease;

      &:hover {
        transform: translateX(4px);
      }
    }

    .duty-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: var(--space-sm);
    }

    .duty-title {
      display: flex;
      align-items: center;
      gap: var(--space-sm);

      h3 {
        font-size: var(--font-size-lg);
        font-weight: 600;
      }
    }

    .duty-rank {
      font-weight: 600;
      color: var(--color-accent-light);
      font-size: var(--font-size-sm);
      padding: 2px 10px;
      background: var(--color-accent-glow);
      border-radius: 999px;
    }

    .duty-dates {
      display: flex;
      align-items: center;
      gap: var(--space-xs);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);

      mat-icon {
        font-size: 16px; width: 16px; height: 16px;
      }
    }

    .error-card, .empty-card {
      text-align: center;
      padding: var(--space-2xl);
      mat-card-content {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-md);
        mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--color-text-muted); }
        p { color: var(--color-text-secondary); }
      }
    }
    .error-card mat-icon { color: var(--color-error) !important; }
  `]
})
export class DutyHistoryComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly dutyService = inject(AstronautDutyService);
    private readonly dialog = inject(MatDialog);
    private readonly snackBar = inject(MatSnackBar);

    readonly person = signal<PersonAstronaut | null>(null);
    readonly duties = signal<AstronautDuty[]>([]);
    readonly loading = signal(true);
    readonly error = signal<string | null>(null);

    personName = '';

    ngOnInit(): void {
        this.personName = this.route.snapshot.paramMap.get('name') ?? '';
        this.loadDuties();
    }

    loadDuties(): void {
        this.loading.set(true);
        this.error.set(null);
        this.dutyService.getDutiesByName(this.personName).subscribe({
            next: (res) => {
                this.person.set(res.person);
                this.duties.set(res.astronautDuties);
                this.loading.set(false);
            },
            error: () => {
                this.error.set('Failed to load duty history.');
                this.loading.set(false);
            }
        });
    }

    openAddDuty(): void {
        const dialogRef = this.dialog.open(AddDutyFormComponent, {
            width: '480px',
            data: { name: this.personName }
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.snackBar.open('Duty assigned successfully', 'Close', { duration: 3000 });
                this.loadDuties();
            }
        });
    }

    formatDate(dateStr: string | null): string {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    }
}
