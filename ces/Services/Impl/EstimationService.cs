﻿using ces.Models;
using Dijkstra.Algorithm.Graphing;
using Dijkstra.Algorithm.Pathing;
using System.Runtime.CompilerServices;
using Route = ces.Models.Route;
using Path = Dijkstra.Algorithm.Pathing.Path;
using ces.DTO.Routes;
using ces.Repositories.Impl;

namespace ces.Services.Impl
{
    public class EstimationService : IEstimationService
    {
        private readonly CityRepository cityRepository;

        public EstimationService(CityRepository cityRepository)
        {
            this.cityRepository = cityRepository;
        }

        public List<Estimation> GetEstimations(string a, string b)
        {
            List<Estimation> estimations = new List<Estimation>();
            estimations.Add(GetPromotedEstimation(a, b));
            estimations.Add(GetCheapestEstimation(a, b));
            estimations.Add(GetShortestEstimation(a, b));
            return estimations;
        }

        private Estimation GetPromotedEstimation(string a, string b)
        {
            var cities = new List<City>();
            var path = GetLocalMap(out cities, EstimationType.Promoted).Build().Dijkstra(a, b);
            var weight = path.Segments.Sum(s => s.Weight);
            var count = path.Segments.Count;
            var price = weight * 3;
            var time = weight * 60 + count * 15;
            return new Estimation() { Cost = price, Time = time, Type = EstimationType.Promoted };
        }

        private Estimation GetCheapestEstimation(string a, string b)
        {
            var cities = new List<City>();
            var path = GetMap(EstimationType.Cheapest).Build().Dijkstra(a, b);
            var price = path.Segments.Sum(s => s.Weight);
            return new Estimation() { Cost = price, Time = 0, Type = EstimationType.Cheapest };
        }

        private Estimation GetShortestEstimation(string a, string b)
        {
            var cities = new List<City>();
            var path = GetMap(EstimationType.Shortest).Build().Dijkstra(a, b);
            var time = path.Segments.Sum(s => s.Weight);
            var count = path.Segments.Count;
            time += count * 15;

            return new Estimation() { Cost = 0, Time = time, Type = EstimationType.Cheapest };
        }

        private GraphBuilder GetCities(out List<City> cities)
        {
            var builder = new GraphBuilder();
            cities = cityRepository.GetCities().Result;
            foreach (City city in cities)
            {
                builder.AddNode(city.Name);
            }
            return builder;
        }

        private GraphBuilder GetLocalMap(out List<City> cities, EstimationType type)
        {
            var builder = GetCities(out cities);

            foreach(City city in cities)
            {
                foreach(Route route in city.Routes)
                {
                    var a = route.Cities.First().Name;
                    var b = route.Cities.Last().Name;

                    if(type.Equals(EstimationType.Cheapest))
                    {
                        builder.AddLink(a, b, route.Distance * 3);
                    } else
                    {
                        builder.AddLink(a, b, route.Distance);
                    }
                }
            }

            return builder;
        }

        private GraphBuilder GetMap(EstimationType type)
        {
            var cities = new List<City>();
            var builder = GetLocalMap(out cities, type);

            foreach (City city in cities)
            {
                var flights = new List<GetRoutesResponse>(); // integration
                var swims = new List<GetRoutesResponse>(); // integration

                foreach (GetRoutesResponse flight in flights)
                {
                    if (type == EstimationType.Cheapest)
                    {
                        builder.AddLink(city.Name, flight.DestCity, flight.Price);
                    }
                    else if (type == EstimationType.Shortest)
                    {
                        builder.AddLink(city.Name, flight.DestCity, flight.Time);
                    }
                }

                foreach (GetRoutesResponse swim in swims)
                {
                    if (type == EstimationType.Cheapest)
                    {
                        builder.AddLink(city.Name, swim.DestCity, swim.Price);
                    }
                    else if (type == EstimationType.Shortest)
                    {
                        builder.AddLink(city.Name, swim.DestCity, swim.Time);
                    }
                }                
            }

            return builder;
        }
    }
}