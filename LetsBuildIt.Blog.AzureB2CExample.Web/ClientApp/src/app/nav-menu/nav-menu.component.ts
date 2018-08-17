import { Component } from '@angular/core';
import { MsalModule, MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(private msalService: MsalService) {
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  public login() {

  }

  public logout() {
    console.log("Logging out");
    this.msalService.logout();
    
    //this.msalService.logout();
  }
}
