import { Component, Input, Output, EventEmitter } from '@angular/core'
import { CustomPoint } from '../custom-point.model'

@Component({
  selector: 'locator-form',
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss'],
})
export class LocatorFormComponent {
  @Input() formTitle: string // ค่าที่สามารถรับเข้ามา
  @Input() geometry: CustomPoint // ค่าที่สามารถรับเข้ามา
  @Output() locate = new EventEmitter<CustomPoint>() // ค่าที่สามารถส่งออกไป

  long: number;
  lat: number;

  ngOnChanges() {
    if (this.geometry) {
      this.lat = this.geometry.latitude;
      this.long = this.geometry.longitude;
    }
  }

  Goto() {
    // เรียกใช้ CustomPoint
    const customPoint = new CustomPoint(this.lat, this.long)
    this.locate.emit(customPoint) // จะคืนค่าเป็น longitude,latitude ไปยัง (locate)="functionที่รองรับค่าที่ส่งไป($...)"
  }
}
