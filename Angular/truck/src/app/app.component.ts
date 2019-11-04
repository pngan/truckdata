import { Component, OnInit } from '@angular/core';
import { RestService } from './rest.service';
import { TruckData, ITruckData } from './truck-data';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'Truck Data Warehouse';
  speed = 0;
  latitude = 0;
  longitude = 0;
  temperature = 0;
  pressure = 0;
  message = 'message';

   tableColumns: string[] = ['companyId', 'truckId', 'speed', 'latitude', 'longtitude', 'temperature', 'pressure', 'driversMessage'];
  truckData: ITruckData[];

  constructor(private restService: RestService) {}

  ngOnInit(): void {
    this.getLatestData();
  }

  insertData() {
    console.log('insertData() pressed: %o', this.message);
    const truckData = new TruckData();
    truckData.customerId = 'freightways';
    truckData.truckId = 'volvo123';
    truckData.speed = this.speed;
    truckData.latitude = this.latitude;
    truckData.longitude = this.longitude;
    truckData.temperature = this.temperature ;
    truckData.pressure = this.pressure;
    truckData.driversMessage = this.message;
    this.restService.insertTruckData(truckData)
      .subscribe(async (response: string) => {
        console.log(`Response = %o`, response);
        await this.delay(500);
        this.getLatestData();
        await this.delay(1000);
        this.getLatestData();
        await this.delay(4000);
        this.getLatestData();
      });
  }

  getLatestData() {
    console.log('getLatestData() pressed: %o', this.message);
    this.restService.getLatestTruckData()
      .subscribe((response: ITruckData[]) => {
        console.log(`Response = %o`, response);
        this.truckData = response;
      });
  }

  ping() {
    console.log('ping() pressed: %o', this.message);
    this.restService.ping()
      .subscribe((response: string) => {
        console.log(`Response = %o`, response);
      });
  }
  delay(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

}
