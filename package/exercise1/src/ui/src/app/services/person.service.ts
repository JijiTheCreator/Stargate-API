import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, retry } from 'rxjs';
import { GetPeopleResponse, GetPersonByNameResponse, CreatePersonResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PersonService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/Person';

    getPeople(): Observable<GetPeopleResponse> {
        return this.http.get<GetPeopleResponse>(this.baseUrl).pipe(retry(2));
    }

    getPersonByName(name: string): Observable<GetPersonByNameResponse> {
        return this.http.get<GetPersonByNameResponse>(`${this.baseUrl}/${encodeURIComponent(name)}`).pipe(retry(2));
    }

    createPerson(name: string): Observable<CreatePersonResponse> {
        return this.http.post<CreatePersonResponse>(this.baseUrl, JSON.stringify(name), {
            headers: { 'Content-Type': 'application/json' }
        });
    }
}
