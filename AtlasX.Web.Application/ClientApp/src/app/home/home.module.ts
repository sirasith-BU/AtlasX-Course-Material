import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core'
import { FormsModule, ReactiveFormsModule } from '@angular/forms'
import { ButtonModule } from 'primeng/button'
import { ToastModule } from 'primeng/toast'

import { CallbackComponent } from './callback/callback.component'
import { HomeRoutingModule } from './home-routing.module'
import { HomeComponent } from './home.component'
import { LoginComponent } from './login/login.component'

@NgModule({
  declarations: [HomeComponent, LoginComponent, CallbackComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, HomeRoutingModule, ButtonModule, ToastModule],
})
export class HomeModule {}
