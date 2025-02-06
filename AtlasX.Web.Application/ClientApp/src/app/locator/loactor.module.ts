import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core'
import { FormsModule } from '@angular/forms'

import { ButtonModule } from 'primeng/button'
import { InputNumberModule } from 'primeng/inputnumber'

import { LocatorRoutingModule } from './locator-routing.module'
import { LocatorComponent } from './locator.component'
import { LocatorFormComponent } from './form/form.component'
import { QueryTaskComponent } from './querytask/query.component'
import { ClosetFacilityComponent } from './closet-facility/clo-fac.component'
import { RouteComponent } from './route/route.component'
import { SketchComponent } from './sketch/sketch.component'
import { LayerListComponent } from './layerList/layer.component'
import { SwipeComponent } from './swipe/swipe.component'

@NgModule({
  declarations: [
    LocatorComponent,
    LocatorFormComponent,
    QueryTaskComponent,
    ClosetFacilityComponent,
    SketchComponent,
    RouteComponent,
    LayerListComponent,
    SwipeComponent,
  ],
  imports: [CommonModule, LocatorRoutingModule, ButtonModule, InputNumberModule, FormsModule],
})
export class LocatorModule {}
