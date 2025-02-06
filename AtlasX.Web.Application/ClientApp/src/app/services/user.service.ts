import { Injectable } from '@angular/core'
import { AxRequestService } from '@atlasx/core/http-service'

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(private requestService: AxRequestService) {}

  getUserData() {
    this.requestService.sp('TEMP_USER_Q', 'GET').subscribe((response: any) => {
      if (response && response.success) {
        return response.data
      }
    })
  }
}
