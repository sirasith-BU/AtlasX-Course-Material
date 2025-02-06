import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'
import TileLayer from '@arcgis/core/layers/TileLayer'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import LayerList from '@arcgis/core/widgets/LayerList'
import Swipe from '@arcgis/core/widgets/Swipe'

@Component({
  selector: 'swap-layer',
  templateUrl: './swipe.component.html',
  styleUrls: ['./swipe.component.scss'],
})
export class SwipeComponent implements AfterViewInit {
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  view!: MapView
  world_ocean_baseURL = 'https://services.arcgisonline.com/arcgis/rest/services/Ocean/World_Ocean_Base/MapServer'
  world_street_mapURL = 'https://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer'
  OceanLayers = new TileLayer({
    url: this.world_ocean_baseURL,
  })
  world_Street = new TileLayer({
    url: this.world_street_mapURL,
  })

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [-99.36306483, 40.01512636],
      zoom: 4,
    })

    map.add(this.OceanLayers)
    map.add(this.world_Street)

    this.view.when(() => {
      let swipe = new Swipe({
        view: this.view,
        leadingLayers: [this.OceanLayers],
        trailingLayers: [this.world_Street],
        direction: 'horizontal',
        position: 50,
      })
      this.view.ui.add(swipe)
    })
  }
}
