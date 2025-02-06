import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'
import MapImageLayer from '@arcgis/core/layers/MapImageLayer'
import TileLayer from '@arcgis/core/layers/TileLayer'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import LayerList from '@arcgis/core/widgets/LayerList'

@Component({
  selector: 'layer-list',
  templateUrl: './layer.component.html',
  styleUrls: ['./layer.component.scss'],
})
export class LayerListComponent implements AfterViewInit {
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  view!: MapView
  world_ocean_baseURL = 'https://services.arcgisonline.com/arcgis/rest/services/Ocean/World_Ocean_Base/MapServer'
  censusURL = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/'
  OceanLayers = new TileLayer({
    url: this.world_ocean_baseURL,
  })
  CensusLayers = new MapImageLayer({
    url: this.censusURL,
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
    map.add(this.CensusLayers)

    this.view.when(() => {
      const layerList = new LayerList({
        view: this.view,
        // container: this.mapPanel.nativeElement,
      })
      layerList.dragEnabled = true
      this.view.ui.add(layerList, {
        position: 'top-right',
      })
    })
  }
}
