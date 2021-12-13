import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StoryData } from '../models/story-data';

@Injectable({
  providedIn: 'root',
})
export class StoryService {
  baseUrl = 'http://localhost:5050/api/stories';

  constructor(private client: HttpClient) {}

  public getStories = (
    pageIndex?: number,
    pageSize?: number,
    title?: string
  ): Observable<StoryData> => {
    const apiUrl = this.buildApiUrl(pageIndex, pageSize, title);

    return this.client.get<StoryData>(apiUrl);
  };

  private buildApiUrl(
    pageIndex?: number,
    pageSize?: number,
    title?: string
  ): string {    
    const paramsBuilder = new URLSearchParams();

    if (pageIndex || pageIndex === 0) {
      paramsBuilder.append('pageIndex', pageIndex.toString());
    }

    if (pageSize || pageSize === 0) {
      paramsBuilder.append('pageSize', pageSize.toString());
    }

    if (title) {
      paramsBuilder.append('title', title.toString());
    }

    return `${this.baseUrl}?${paramsBuilder.toString()}`;
  }
}
