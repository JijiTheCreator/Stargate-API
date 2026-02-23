import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, retry } from 'rxjs';
import { GetAstronautDutiesByNameResponse, CreateAstronautDutyRequest, CreateAstronautDutyResponse } from '../models/api.models';

/**
 * Service for managing Astronaut Duty assignments via the Stargate API.
 * Provides operations to retrieve duty history and assign new duties.
 */
@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/AstronautDuty';

    /**
     * Retrieves all astronaut duty assignments for a person.
     * @param name - The name of the person whose duties to retrieve.
     */
    getDutiesByName(name: string): Observable<GetAstronautDutiesByNameResponse> {
        return this.http.get<GetAstronautDutiesByNameResponse>(`${this.baseUrl}/${encodeURIComponent(name)}`).pipe(retry(2));
    }

    /**
     * Creates a new astronaut duty assignment. Enforces business rules R2–R7.
     * @param request - The duty assignment details (name, rank, title, start date).
     */
    createDuty(request: CreateAstronautDutyRequest): Observable<CreateAstronautDutyResponse> {
        return this.http.post<CreateAstronautDutyResponse>(this.baseUrl, request);
    }
}
