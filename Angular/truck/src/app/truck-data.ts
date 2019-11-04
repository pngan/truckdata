export interface ITruckData {
    customerId: string;
    truckId: string;

    speed: number;
    latitude: number;
    longitude: number;
    temperature: number;
    pressure: number;
    driversMessage: string;
}

export class TruckData implements ITruckData {
    customerId: string;
    truckId: string;
    speed = 0;
    latitude = 0;
    longitude = 0;
    temperature = 0;
    pressure = 0;
    driversMessage = 'message';
}
