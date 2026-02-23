/** Standard API response envelope with success status and message. */
export interface BaseResponse {
    /** Whether the request was successful. */
    success: boolean;
    /** Human-readable status or error message. */
    message: string;
    /** HTTP-like status code (200, 400, 404, 500, etc.). */
    responseCode: number;
}

/** A person with their current astronaut detail (if assigned). */
export interface PersonAstronaut {
    /** Unique person identifier. */
    personId: number;
    /** Unique name of the person (business rule R1). */
    name: string;
    /** Current astronaut rank, or empty if not an astronaut. */
    currentRank: string;
    /** Current duty title, or empty if not an astronaut. */
    currentDutyTitle: string;
    /** Career start date (ISO string), or null if no astronaut record. */
    careerStartDate: string | null;
    /** Career end date (ISO string), or null if still active. */
    careerEndDate: string | null;
}

/** A single astronaut duty assignment record. */
export interface AstronautDuty {
    /** Unique duty record identifier. */
    id: number;
    /** Foreign key to the Person entity. */
    personId: number;
    /** Astronaut rank during this duty. */
    rank: string;
    /** Duty title (e.g., "Commander", "RETIRED"). */
    dutyTitle: string;
    /** Duty start date (ISO string). */
    dutyStartDate: string;
    /** Duty end date (ISO string), or null if this is the current duty. */
    dutyEndDate: string | null;
}

/** Response from GET /Person — returns all people. */
export interface GetPeopleResponse extends BaseResponse {
    /** List of all people with their astronaut details. */
    people: PersonAstronaut[];
}

/** Response from GET /Person/{name} — returns a single person. */
export interface GetPersonByNameResponse extends BaseResponse {
    /** The person with astronaut detail, or null if not found. */
    person: PersonAstronaut | null;
}

/** Response from POST /Person — returns the created person's ID. */
export interface CreatePersonResponse extends BaseResponse {
    /** The database ID of the newly created person. */
    id: number;
}

/** Response from GET /AstronautDuty/{name} — returns duty history. */
export interface GetAstronautDutiesByNameResponse extends BaseResponse {
    /** The person's current astronaut detail, or null if not found. */
    person: PersonAstronaut | null;
    /** Complete list of all duty assignments for this person. */
    astronautDuties: AstronautDuty[];
}

/** Request body for POST /AstronautDuty — creates a new duty assignment. */
export interface CreateAstronautDutyRequest {
    /** The name of the person to assign the duty to. */
    name: string;
    /** The astronaut rank for this assignment. */
    rank: string;
    /** The duty title (use "RETIRED" to retire the astronaut). */
    dutyTitle: string;
    /** The start date for this duty (ISO string format). */
    dutyStartDate: string;
}

/** Response from POST /AstronautDuty — returns the created duty's ID. */
export interface CreateAstronautDutyResponse extends BaseResponse {
    /** The database ID of the newly created duty record, or null on failure. */
    id: number | null;
}
