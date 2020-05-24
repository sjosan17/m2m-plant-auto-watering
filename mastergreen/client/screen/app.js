import React from 'react';
import dotnetify from 'dotnetify';
import Iframe from 'react-iframe';
//import 'bootstrap/dist/css/bootstrap.min.css';
import Button from 'react-bootstrap/Button';

export default class HelloWorld extends React.Component {
    constructor(props) {
        super(props);
        this.vm = dotnetify.react.connect('MainAppVM', this);
        this.state = { Greetings: '', ServerTime: '' };
        
        this.dispatchState = state => {
            this.setState(state);
            this.vm.$dispatch(state);
        };
    }

    render() {
        let devices = [];
        if (this.state.Devices) {
            devices = this.state.Devices;
        }
        return (
        <div>
            <div>
                <div>Devices:{devices.length}</div>
                <div>{devices.map(item => <div>
                    Realtime data from: {item.deviceId} - soil: {item.SoilMoisture} - temp: {item.Temperature} - hum: {item.Humidity} - light:{item.Light}
                    <Iframe src="http://YOURSERVER:8882/app/kibana#/dashboard/a5967410-139f-11ea-a497-a52b03ce8aaf?embed=true&_g=(filters%3A!()%2CrefreshInterval%3A(pause%3A!f%2Cvalue%3A15000)%2Ctime%3A(from%3Anow-24h%2Cto%3Anow))" height="99%" width="100%">
                    </Iframe>
                </div>)}
                </div>
            </div>
            <div>
                <div>Manually water the plant:
                <Button variant="primary" onClick={_ => this.dispatchState({ Water: 1 })}>
                    Fill Water
                </Button>
                </div>
            </div>
        </div>
        );
    }
}