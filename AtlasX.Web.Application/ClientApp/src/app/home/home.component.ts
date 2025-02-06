import { Component, Inject, OnInit } from '@angular/core'

import { AxAuthenticationService } from '@atlasx/core/authentication'
import { AxConfigurationService } from '@atlasx/core/configuration'

import { AxRequestService, AxWebServiceUrl } from '@atlasx/core/http-service'

import { MessageService } from 'primeng/api'
import { UserService } from '../services/user.service'

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  providers: [MessageService],
})
export class HomeComponent implements OnInit {
  users: any[] = []

  constructor(
    public configService: AxConfigurationService,
    public authService: AxAuthenticationService,
    private messageService: MessageService,
    private requestService: AxRequestService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    // this.users = this.userService.getUserData()

    this.requestService
      .request('https://localhost:5001/api/apptest/hello', 'POST', {
        name: 'John',
      })
      .subscribe((response: any) => {
        if (response && response.success) {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: response.message })
        }
      })
    this.requestService
      .request('https://localhost:5001/api/apphoro?name=Sirsaith', 'GET')
      .subscribe((response: any) => {
        if (response) {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: response.grade })
        } else {
          this.messageService.add({ severity: 'danger', summary: 'Error', detail: 'Error' })
        }
      })

    
  }

  addNewUser() {
    this.requestService
      .sp('TEMP_USER_I', 'POST', {
        NAME: 'Jakie',
        SURNAME: 'Doe',
        GENDER: 'M',
        MOBILE: '0999999999',
        LATITUDE: 13.2,
        LONGITUDE: 100.3,
      })
      .subscribe((response: any) => {
        if (response && response.success) {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'New User Added!' })
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: "Can't Add New User!" })
        }
      })
  }

  showSuccess() {
    this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Message Content' })
  }

  showInfo() {
    this.messageService.add({ severity: 'info', summary: 'Info', detail: 'Message Content' })
  }

  showWarn() {
    this.messageService.add({ severity: 'warn', summary: 'Warn', detail: 'Message Content' })
  }

  showError() {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Message Content' })
  }

  showContrast() {
    this.messageService.add({ severity: 'contrast', summary: 'Error', detail: 'Message Content' })
  }

  showSecondary() {
    this.messageService.add({ severity: 'secondary', summary: 'Secondary', detail: 'Message Content' })
  }
}
