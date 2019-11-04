import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { ITruckData } from './truck-data';

@Injectable({
  providedIn: 'root'
})
export class RestService {
  //baseUrl = 'http://localhost:54658/';
  baseUrl = 'https://truckdataapp.azurewebsites.net/';
  //baseUrl = 'https://truckstatic.z22.web.core.windows.net/';
  constructor(private http: HttpClient) { }

  insertTruckData(truckData: ITruckData): Observable<string> {
    const headersRequest = {
      'Content-Type': 'application/json'
    };
    console.log(`%o %o`, truckData, `${this.baseUrl}api/truckdata`);
    return this.http.post<string>(`${this.baseUrl}api/truckdata`, JSON.stringify(truckData), { headers: headersRequest });
  }


  getLatestTruckData(): Observable<ITruckData[]> {
    const headersRequest = {
      'Content-Type': 'application/json'
    };
    console.log(`GET ${this.baseUrl}api/truckdata`);
    return this.http.get<ITruckData[]>(`${this.baseUrl}api/truckdata?customerId=freightways&truckId=volvo123`);
  }

  ping(): Observable<string> {
    console.log(`GET ${this.baseUrl}api/truckdata/ping`);
    return this.http.get<string>(`${this.baseUrl}api/truckdata/ping`);
  }
}
