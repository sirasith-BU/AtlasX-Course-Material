import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { UserMComponent } from './um.component'

const routes: Routes = [
  {
    path: '',
    component: UserMComponent,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UserMRoutingModule {}
