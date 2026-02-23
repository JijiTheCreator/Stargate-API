import { Component, ChangeDetectionStrategy, signal, computed, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { PersonService } from '../../services/person.service';
import { PersonAstronaut } from '../../models/api.models';
import { AddPersonDialogComponent } from '../add-person-dialog/add-person-dialog.component';

@Component({
    selector: 'app-people-list',
    standalone: true,
    imports: [
        MatTableModule, MatCardModule, MatInputModule, MatFormFieldModule,
        MatIconModule, MatButtonModule, MatProgressBarModule, MatSnackBarModule,
        MatDialogModule, FormsModule
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <div class="page-container">
      <div class="page-header">
        <div class="header-row">
          <div>
            <h1>Personnel Registry</h1>
            <p>Astronaut Career Tracking System — All registered personnel</p>
          </div>
          <button mat-fab extended color="primary" (click)="openAddPerson()" aria-label="Add new person">
            <mat-icon>person_add</mat-icon>
            Add Person
          </button>
        </div>
      </div>

      @if (loading()) {
        <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
      }

      @if (error()) {
        <mat-card class="error-card">
          <mat-card-content>
            <mat-icon>error_outline</mat-icon>
            <p>{{ error() }}</p>
            <button mat-stroked-button (click)="loadPeople()">
              <mat-icon>refresh</mat-icon> Retry
            </button>
          </mat-card-content>
        </mat-card>
      }

      @if (!loading() && !error()) {
        <mat-form-field class="search-field" appearance="outline">
          <mat-label>Search personnel</mat-label>
          <mat-icon matPrefix>search</mat-icon>
          <input matInput [ngModel]="searchTerm()" (ngModelChange)="searchTerm.set($event)" placeholder="Filter by name, rank, or duty...">
        </mat-form-field>

        @if (filteredPeople().length === 0) {
          <mat-card class="empty-card">
            <mat-card-content>
              <mat-icon>group_off</mat-icon>
              <p>No personnel found</p>
            </mat-card-content>
          </mat-card>
        } @else {
          <div class="people-grid">
            @for (person of filteredPeople(); track person.personId) {
              <mat-card class="person-card" (click)="viewPerson(person)" tabindex="0"
                        (keydown.enter)="viewPerson(person)" role="button"
                        [attr.aria-label]="'View details for ' + person.name">
                <div class="card-header">
                  <div class="avatar">{{ person.name.charAt(0) }}</div>
                  <div class="card-info">
                    <h3>{{ person.name }}</h3>
                    <span class="status-badge" [class]="getStatusClass(person)">
                      {{ person.currentDutyTitle || 'Civilian' }}
                    </span>
                  </div>
                </div>
                <div class="card-details">
                  @if (person.currentRank) {
                    <div class="detail-item">
                      <mat-icon>military_tech</mat-icon>
                      <span>{{ person.currentRank }}</span>
                    </div>
                  }
                  @if (person.careerStartDate) {
                    <div class="detail-item">
                      <mat-icon>calendar_today</mat-icon>
                      <span>Since {{ formatDate(person.careerStartDate) }}</span>
                    </div>
                  }
                </div>
              </mat-card>
            }
          </div>
        }
      }
    </div>
  `,
    styles: [`
    .header-row {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-md);
    }

    .search-field {
      width: 100%;
      margin-bottom: var(--space-lg);
    }

    .people-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: var(--space-lg);
    }

    .person-card {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      cursor: pointer;

      &:hover, &:focus-visible {
        transform: translateY(-2px);
        box-shadow: var(--shadow-glow), var(--shadow-md);
        outline: none;
      }
    }

    .card-header {
      display: flex;
      align-items: center;
      gap: var(--space-md);
      padding: var(--space-lg);
      padding-bottom: var(--space-sm);
    }

    .avatar {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--color-accent), var(--color-info));
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: var(--font-size-xl);
      font-weight: 700;
      color: white;
      flex-shrink: 0;
    }

    .card-info {
      h3 {
        font-size: var(--font-size-lg);
        font-weight: 600;
        color: var(--color-text-primary);
        margin-bottom: var(--space-xs);
      }
    }

    .card-details {
      display: flex;
      gap: var(--space-lg);
      padding: var(--space-sm) var(--space-lg) var(--space-lg);

      .detail-item {
        display: flex;
        align-items: center;
        gap: var(--space-xs);
        color: var(--color-text-secondary);
        font-size: var(--font-size-sm);

        mat-icon {
          font-size: 16px;
          width: 16px;
          height: 16px;
        }
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

        mat-icon {
          font-size: 48px;
          width: 48px;
          height: 48px;
          color: var(--color-text-muted);
        }

        p { color: var(--color-text-secondary); }
      }
    }

    .error-card mat-icon { color: var(--color-error) !important; }
  `]
})
export class PeopleListComponent implements OnInit {
    private readonly personService = inject(PersonService);
    private readonly router = inject(Router);
    private readonly snackBar = inject(MatSnackBar);
    private readonly dialog = inject(MatDialog);

    readonly people = signal<PersonAstronaut[]>([]);
    readonly loading = signal(true);
    readonly error = signal<string | null>(null);
    readonly searchTerm = signal('');

    readonly filteredPeople = computed(() => {
        const term = this.searchTerm().toLowerCase();
        if (!term) return this.people();
        return this.people().filter(p =>
            p.name.toLowerCase().includes(term) ||
            (p.currentRank && p.currentRank.toLowerCase().includes(term)) ||
            (p.currentDutyTitle && p.currentDutyTitle.toLowerCase().includes(term))
        );
    });

    ngOnInit(): void {
        this.loadPeople();
    }

    loadPeople(): void {
        this.loading.set(true);
        this.error.set(null);
        this.personService.getPeople().subscribe({
            next: (res) => {
                this.people.set(res.people);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set('Failed to load personnel. Please try again.');
                this.loading.set(false);
            }
        });
    }

    viewPerson(person: PersonAstronaut): void {
        this.router.navigate(['/people', person.name]);
    }

    openAddPerson(): void {
        const dialogRef = this.dialog.open(AddPersonDialogComponent, {
            width: '400px'
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.snackBar.open(`Person "${result}" added successfully`, 'Close', { duration: 3000 });
                this.loadPeople();
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
