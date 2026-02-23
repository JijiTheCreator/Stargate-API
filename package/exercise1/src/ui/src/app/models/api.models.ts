export interface BaseResponse {
    success: boolean;
    message: string;
    responseCode: number;
}

export interface PersonAstronaut {
    personId: number;
    name: string;
    currentRank: string;
    currentDutyTitle: string;
    careerStartDate: string | null;
    careerEndDate: string | null;
}

export interface AstronautDuty {
    id: number;
    personId: number;
    rank: string;
    dutyTitle: string;
    dutyStartDate: string;
    dutyEndDate: string | null;
}

export interface GetPeopleResponse extends BaseResponse {
    people: PersonAstronaut[];
}

export interface GetPersonByNameResponse extends BaseResponse {
    person: PersonAstronaut | null;
}

export interface CreatePersonResponse extends BaseResponse {
    id: number;
}

export interface GetAstronautDutiesByNameResponse extends BaseResponse {
    person: PersonAstronaut | null;
    astronautDuties: AstronautDuty[];
}

export interface CreateAstronautDutyRequest {
    name: string;
    rank: string;
    dutyTitle: string;
    dutyStartDate: string;
}

export interface CreateAstronautDutyResponse extends BaseResponse {
    id: number | null;
}
