import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class KittenService {
  baseUrl = 'https://placekitten.com';
  width = 600;
  height = 500;

  constructor(private client: HttpClient) {}

  public getKitten(width?: number, height?: number): Observable<Blob> {
    const apiUrl = this.buildKittenUrl(width, height);

    return this.client.get(apiUrl, { responseType: 'blob' });
  }

  private buildKittenUrl(width?: number, height?: number): string {
    let apiUrl = this.baseUrl + '/';

    apiUrl += width && width >= this.width ? width : this.width;
    apiUrl += '/';
    apiUrl += height && height >= this.height ? height : this.height;

    return apiUrl;
  }
}
