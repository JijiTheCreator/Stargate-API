import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, retry } from 'rxjs';
import { GetPeopleResponse, GetPersonByNameResponse, CreatePersonResponse } from '../models/api.models';

/**
 * Service for managing Person entities via the Stargate API.
 * Provides CRUD operations for people tracked in the ACTS system.
 */
@Injectable({ providedIn: 'root' })
export class PersonService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/Person';

    /** Retrieves all people, including their current astronaut detail if assigned. */
    getPeople(): Observable<GetPeopleResponse> {
        return this.http.get<GetPeopleResponse>(this.baseUrl).pipe(retry(2));
    }

    /**
     * Retrieves a single person by name.
     * @param name - The unique name of the person to retrieve.
     */
    getPersonByName(name: string): Observable<GetPersonByNameResponse> {
        return this.http.get<GetPersonByNameResponse>(`${this.baseUrl}/${encodeURIComponent(name)}`).pipe(retry(2));
    }

    /**
     * Creates a new person. Names must be unique (business rule R1).
     * @param name - The unique name for the new person.
     */
    createPerson(name: string): Observable<CreatePersonResponse> {
        return this.http.post<CreatePersonResponse>(this.baseUrl, JSON.stringify(name), {
            headers: { 'Content-Type': 'application/json' }
        });
    }
}
