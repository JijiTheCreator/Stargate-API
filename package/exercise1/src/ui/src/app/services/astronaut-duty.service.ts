import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, retry } from 'rxjs';
import { GetAstronautDutiesByNameResponse, CreateAstronautDutyRequest, CreateAstronautDutyResponse } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/AstronautDuty';

    getDutiesByName(name: string): Observable<GetAstronautDutiesByNameResponse> {
        return this.http.get<GetAstronautDutiesByNameResponse>(`${this.baseUrl}/${encodeURIComponent(name)}`).pipe(retry(2));
    }

    createDuty(request: CreateAstronautDutyRequest): Observable<CreateAstronautDutyResponse> {
        return this.http.post<CreateAstronautDutyResponse>(this.baseUrl, request);
    }
}
