import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import Point from '@arcgis/core/geometry/Point'
import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol'
import Graphic from '@arcgis/core/Graphic'
import MapImageLayer from '@arcgis/core/layers/MapImageLayer'
import * as urlUtils from '@arcgis/core/core/urlUtils'

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.scss'],
})
export class MapComponent implements AfterViewInit {
  @ViewChild('mapPanel', { static: true }) mapPanel: ElementRef
  constructor() {
    urlUtils.addProxyRule({
      proxyUrl: 'https://localhost:5001/api/appproxy',
      urlPrefix: 'https://gisserv1.cdg.co.th/arcgis/rest/services',
    })
  }

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    const USALayers = new MapImageLayer({
      url: 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer',
    })

    const atlasXSecureLayer = new MapImageLayer({
      url: 'https://gisserv1.cdg.co.th/arcgis/rest/services/AtlasX/AtlasX_Secure/MapServer',
    })

    // map.add(USALayers)
    map.add(atlasXSecureLayer)

    const view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [100, 13],
      zoom: 15,
    })

    view.when(() => {
      console.log('MapView is ready!')

      const point = new Point({
        longitude: 100,
        latitude: 13,
      })

      const marker = new SimpleMarkerSymbol({
        color: [255, 128, 0],
        outline: {
          color: [255, 255, 255],
          width: 1,
        },
      })

      const pointGraphic = new Graphic({
        geometry: point,
        symbol: marker,
      })

      view.graphics.add(pointGraphic)
    })
  }
}
