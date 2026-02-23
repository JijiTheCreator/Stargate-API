import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'people', pathMatch: 'full' },
    {
        path: 'people',
        loadComponent: () => import('./components/people-list/people-list.component').then(m => m.PeopleListComponent)
    },
    {
        path: 'people/:name',
        loadComponent: () => import('./components/person-detail/person-detail.component').then(m => m.PersonDetailComponent)
    },
    {
        path: 'duties/:name',
        loadComponent: () => import('./components/duty-history/duty-history.component').then(m => m.DutyHistoryComponent)
    },
    { path: '**', redirectTo: 'people' }
];
