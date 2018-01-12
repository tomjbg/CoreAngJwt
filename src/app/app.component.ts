import { Component } from '@angular/core';
import { Http } from '@angular/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent {
  title = 'app';
  lst: string[];

  /**
   *
   */
  constructor(http: Http) {
  }
}

Busca(): string {
  return 'Vai que cola';
}

BuscaLista(): void {

  this.http.get('Home/ListaPalavras')
    .subscribe(l => this.lst.push(l.json()));

  this.lst.forEach(txt => {
    console.log(txt);
  });

}
