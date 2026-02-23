import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { PersonService } from '../../services/person.service';

@Component({
    selector: 'app-add-person-dialog',
    standalone: true,
    imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, FormsModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <h2 mat-dialog-title>Add New Person</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Full Name</mat-label>
        <input matInput [(ngModel)]="name" placeholder="e.g. Jane Doe" required>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="!name.trim()" (click)="submit()">
        Add Person
      </button>
    </mat-dialog-actions>
  `,
    styles: [`
    .full-width { width: 100%; }
    mat-dialog-content { padding-top: var(--space-md) !important; }
  `]
})
export class AddPersonDialogComponent {
    private readonly dialogRef = inject(MatDialogRef<AddPersonDialogComponent>);
    private readonly personService = inject(PersonService);

    name = '';

    submit(): void {
        if (!this.name.trim()) return;
        this.personService.createPerson(this.name.trim()).subscribe({
            next: () => this.dialogRef.close(this.name.trim()),
            error: (err) => {
                const msg = err?.error?.message || 'Failed to add person';
                alert(msg);
            }
        });
    }
}
