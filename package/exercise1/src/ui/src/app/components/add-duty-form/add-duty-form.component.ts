import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AstronautDutyService } from '../../services/astronaut-duty.service';

@Component({
    selector: 'app-add-duty-form',
    standalone: true,
    imports: [
        MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule,
        MatDatepickerModule, MatNativeDateModule, ReactiveFormsModule
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
    <h2 mat-dialog-title>Assign New Duty</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="duty-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Rank</mat-label>
          <input matInput formControlName="rank" placeholder="e.g. CPT, MAJ, COL">
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Duty Title</mat-label>
          <input matInput formControlName="dutyTitle" placeholder="e.g. Commander, Pilot, RETIRED">
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Duty Start Date</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="dutyStartDate">
          <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || submitting" (click)="submit()">
        {{ submitting ? 'Assigning...' : 'Assign Duty' }}
      </button>
    </mat-dialog-actions>
  `,
    styles: [`
    .duty-form {
      display: flex;
      flex-direction: column;
      gap: var(--space-sm);
      padding-top: var(--space-md);
    }
    .full-width { width: 100%; }
  `]
})
export class AddDutyFormComponent {
    private readonly dialogRef = inject(MatDialogRef<AddDutyFormComponent>);
    private readonly dutyService = inject(AstronautDutyService);
    private readonly fb = inject(FormBuilder);
    private readonly data: { name: string } = inject(MAT_DIALOG_DATA);

    submitting = false;

    form = this.fb.group({
        rank: ['', Validators.required],
        dutyTitle: ['', Validators.required],
        dutyStartDate: [null as Date | null, Validators.required]
    });

    submit(): void {
        if (this.form.invalid) return;
        this.submitting = true;

        const val = this.form.value;
        const startDate = val.dutyStartDate ? new Date(val.dutyStartDate).toISOString() : '';

        this.dutyService.createDuty({
            name: this.data.name,
            rank: val.rank!,
            dutyTitle: val.dutyTitle!,
            dutyStartDate: startDate
        }).subscribe({
            next: () => this.dialogRef.close(true),
            error: (err) => {
                this.submitting = false;
                const msg = err?.error?.message || 'Failed to assign duty';
                alert(msg);
            }
        });
    }
}
