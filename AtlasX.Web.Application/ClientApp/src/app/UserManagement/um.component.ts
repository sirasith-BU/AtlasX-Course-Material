import { Component, AfterViewInit, ViewChild, ElementRef, OnInit } from '@angular/core'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import Point from '@arcgis/core/geometry/Point'
import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol'
import Graphic from '@arcgis/core/Graphic'
import { AxRequestService } from '@atlasx/core/http-service'
import { MessageService, ConfirmationService } from 'primeng/api'
import { CustomPoint } from '../locator/custom-point.model'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { catchError } from 'rxjs/operators'
import { throwError } from 'rxjs'

@Component({
  selector: 'user-manage',
  templateUrl: './um.component.html',
  styleUrls: ['./um.component.scss'],
  providers: [MessageService, ConfirmationService],
})
export class UserMComponent implements OnInit, AfterViewInit {
  constructor(
    private requestService: AxRequestService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  @ViewChild('mapPanel', { static: true }) mapPanel: ElementRef
  users: any[] = []
  filteredUsers: any = []
  searchUser: any = []
  searchText: String = ''
  rating: number = 0
  starsArray = Array(3).fill(0)
  latitude: number = 13.703050476459964
  longitude: number = 100.54343247028669
  view!: MapView
  clickPoint: CustomPoint
  svgPath =
    'M213.285,0h-0.608C139.114,0,79.268,59.826,79.268,133.361c0,48.202,21.952,111.817,65.246,189.081 c32.098,57.281,64.646,101.152,64.972,101.588c0.906,1.217,2.334,1.934,3.847,1.934c0.043,0,0.087,0,0.13-0.002 c1.561-0.043,3.002-0.842,3.868-2.143c0.321-0.486,32.637-49.287,64.517-108.976c43.03-80.563,64.848-141.624,64.848-181.482 C346.693,59.825,286.846,0,213.285,0z M274.865,136.62c0,34.124-27.761,61.884-61.885,61.884 c-34.123,0-61.884-27.761-61.884-61.884s27.761-61.884,61.884-61.884C247.104,74.736,274.865,102.497,274.865,136.62z'
  marker = new SimpleMarkerSymbol({
    path: this.svgPath,
    color: [255, 0, 0],
    size: 30,
    outline: {
      color: [255, 255, 255],
      width: 1,
    },
  })

  userForm = new FormGroup({
    NAME: new FormControl('', [Validators.required]),
    SURNAME: new FormControl('', [Validators.required]),
    GENDER: new FormControl('', [Validators.required]),
    MOBILE: new FormControl('', [Validators.required, Validators.minLength(10), Validators.maxLength(10)]),
  })

  ngOnInit(): void {
    this.loadUser()
  }

  async loadUser() {
    await this.requestService.sp('TEMP_USER_Q', 'GET').subscribe((response: any) => {
      if (response && response.success) {
        this.users = response.data
        this.filteredUsers = this.users
        this.messageService.add({ severity: 'success', summary: 'สำเร็จ', detail: 'โหลดข้อมูลสำเร็จ!' })
      } else {
        this.messageService.add({ severity: 'danger', summary: 'ผิดพลาด', detail: 'เกิดข้อผิดพลาด!' })
      }
    })
  }

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [this.longitude, this.latitude],
      zoom: 19,
    })

    this.view.when(() => {
      console.log('MapView is ready!')

      const point = new Point({
        longitude: this.longitude,
        latitude: this.latitude,
      })

      const pointGraphic = new Graphic({
        geometry: point,
        symbol: this.marker,
      })

      this.view.graphics.add(pointGraphic)

      // Click event MapView
      this.view.on('click', (event) => sendMapPoint(event)) // Send Longitdue,Latitude

      const sendMapPoint = (event) => {
        const customPoint = new CustomPoint(event.mapPoint.latitude, event.mapPoint.longitude)
        // this.clickPoint = customPoint
        this.longitude = customPoint.longitude
        this.latitude = customPoint.latitude

        const point = new Point({
          longitude: this.longitude,
          latitude: this.latitude,
        })

        const pointGraphic = new Graphic({
          geometry: point,
          symbol: this.marker,
        })

        this.view.graphics.removeAll()
        this.view.graphics.add(pointGraphic)
        this.view.goTo({
          target: point,
        })
      }
    })
  }

  searchUsers() {
    const lowerCaseSearchText = this.searchText.toLowerCase()

    this.filteredUsers = this.users.filter((user) => {
      const fullName = `${user.NAME} ${user.SURNAME}`.toLowerCase()
      return fullName.includes(lowerCaseSearchText)
    })
  }

  selectUser(user) {
    this.searchUser = user

    this.userForm.setValue({
      NAME: this.searchUser.NAME,
      SURNAME: this.searchUser.SURNAME,
      GENDER: this.searchUser.GENDER,
      MOBILE: this.searchUser.MOBILE,
    })

    this.clickPoint = new CustomPoint(user.LATITUDE, user.LONGITUDE)

    const point = new Point({
      longitude: user.LONGITUDE,
      latitude: user.LATITUDE,
    })

    const pointGraphic = new Graphic({
      geometry: point,
      symbol: this.marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(pointGraphic)
    this.view.goTo({
      target: point,
      zoom: 19,
    })

    this.requestService
      .request(`https://localhost:5001/api/apphoro?name=${this.searchUser.NAME}`, 'GET')
      .pipe(
        catchError((error) => {
          console.log(error)
          if (error.status === 400) {
            this.messageService.add({
              severity: 'warn',
              summary: 'คำเตือน',
              detail: error.error || 'การร้องขอไม่ผ่าน!',
            })
          }
          // ส่งกลับ throwError เพื่อให้ Observable ดำเนินการต่อ
          return throwError(() => error)
        })
      )
      .subscribe((response: any) => {
        if (response) {
          this.rating = response.stars
          // console.log(response)
        } else {
          this.messageService.add({
            severity: 'warn',
            summary: 'คำเตือน',
            detail: 'การร้องขอไม่ผ่าน!',
          })
        }
      })
  }

  rateUsers() {
    const nameForm = this.userForm.get('NAME')?.value
    if (nameForm) {
      this.requestService
        .request(`https://localhost:5001/api/apphoro?name=${nameForm}`, 'GET')
        .pipe(
          catchError((error) => {
            if (error.status === 400) {
              this.messageService.add({
                severity: 'warn',
                summary: 'คำเตือน',
                detail: error.error || 'การร้องขอไม่ผ่าน!',
              })
            }
            // ส่งกลับ throwError เพื่อให้ Observable ดำเนินการต่อ
            return throwError(() => error)
          })
        )
        .subscribe((response: any) => {
          if (response) {
            this.rating = response.stars
            // console.log(response)
          } else {
            this.messageService.add({
              severity: 'warn',
              summary: 'คำเตือน',
              detail: 'การร้องขอไม่ผ่าน!',
            })
          }
        })
    } else {
      this.rating = 0
    }
  }

  addUser() {
    if (this.userForm.valid) {
      const formValue = this.userForm.value
      // INSERT
      this.requestService
        .sp('TEMP_USER_I', 'POST', {
          NAME: formValue.NAME,
          SURNAME: formValue.SURNAME,
          GENDER: formValue.GENDER,
          MOBILE: formValue.MOBILE,
          LONGITUDE: this.longitude,
          LATITUDE: this.latitude,
        })
        .subscribe((response: any) => {
          if (response && response.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'สำเร็จ',
              detail: 'บันทึก ข้อมูลผู้ใช้สำเร็จ!',
            })
          } else {
            this.messageService.add({ severity: 'danger', summary: 'ผิดพลาด', detail: 'เกิดข้อผิดพลาด!' })
          }
        })
      this.resetForm()
      this.loadUser()
    } else {
      this.messageService.add({ severity: 'warn', summary: 'คำเตือน', detail: 'กรุณากรอกข้อมูลให้ถูกต้อง!' })
    }
  }

  updateUser() {
    if (this.userForm.valid) {
      const formValue = this.userForm.value
      // UPDATE
      this.requestService
        .sp('TEMP_USER_U', 'POST', {
          USER_ID: this.searchUser.USER_ID,
          NAME: formValue.NAME,
          SURNAME: formValue.SURNAME,
          GENDER: formValue.GENDER,
          MOBILE: formValue.MOBILE,
          LONGITUDE: this.longitude,
          LATITUDE: this.latitude,
        })
        .subscribe((response: any) => {
          if (response && response.success) {
            this.messageService.add({
              severity: 'success',
              summary: 'สำเร็จ',
              detail: 'อัพเดท ข้อมูลผู้ใช้สำเร็จ!',
            })
          } else {
            this.messageService.add({ severity: 'danger', summary: 'ผิดพลาด', detail: 'เกิดข้อผิดพลาด!' })
          }
        })
      this.resetForm()
      this.loadUser()
    } else {
      this.messageService.add({ severity: 'warn', summary: 'คำเตือน', detail: 'กรุณากรอกข้อมูลให้ถูกต้อง!' })
    }
  }

  resetForm() {
    this.searchUser = []
    this.userForm.reset()
    const point = new Point({
      longitude: 100.54343247028669,
      latitude: 13.703050476459964,
    })

    const pointGraphic = new Graphic({
      geometry: point,
      symbol: this.marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(pointGraphic)
    this.view.goTo({
      target: point,
      zoom: 19,
    })
    this.rating = 0
  }

  deleteUser() {
    if (this.searchUser.USER_ID) {
      this.confirmationService.confirm({
        message: 'คุณต้องการลบข้อมูลผู้ใช้?',
        header: 'การลบข้อมูลผู้ใช้',
        icon: 'pi pi-exclamation-triangle',
        acceptIcon: 'none',
        rejectIcon: 'none',
        acceptButtonStyleClass: 'p-button-danger p-button-text',
        rejectButtonStyleClass: 'p-button-text p-button-text',
        accept: () => {
          this.requestService
            .sp('TEMP_USER_D', 'POST', {
              USER_ID: this.searchUser.USER_ID,
            })
            .subscribe((response: any) => {
              if (response && response.success) {
                this.messageService.add({ severity: 'success', summary: 'สำเร็จ', detail: 'ลบข้อมูลผู้ใช้สำเร็จ!' })
              } else {
                this.messageService.add({ severity: 'danger', summary: 'ผิดพลาด', detail: 'เกิดข้อผิดพลาด!' })
              }
            })
          this.resetForm()
          this.loadUser()
        },
        reject: () => {},
      })
    } else {
      this.messageService.add({ severity: 'warn', summary: 'คำเตือน', detail: 'กรุณาเลือกผู้ใช้' })
    }
  }
}
